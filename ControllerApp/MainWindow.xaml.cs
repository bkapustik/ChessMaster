using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace ControllerApp
{
    public class RawMove
    {
        private static readonly Dictionary<string, Chess.PieceType> pieceDesignations = new Dictionary<string, Chess.PieceType> {
            { "p", Chess.PieceType.Pawn },
            { "r", Chess.PieceType.Rook},
            { "n", Chess.PieceType.Knight },
            { "b", Chess.PieceType.Bishop },
            { "k", Chess.PieceType.King },
            { "q", Chess.PieceType.Queen },
        };

        private static readonly Dictionary<Chess.PieceType, PieceHeight> pieceHeights = new Dictionary<Chess.PieceType, PieceHeight> {
            { Chess.PieceType.Pawn, PieceHeight.Small },
            { Chess.PieceType.Rook, PieceHeight.Medium },
            { Chess.PieceType.Knight, PieceHeight.Medium },
            { Chess.PieceType.Bishop, PieceHeight.Medium },
            { Chess.PieceType.King, PieceHeight.High },
            { Chess.PieceType.Queen, PieceHeight.High },
        };


        public readonly int FromX;
        public readonly int FromY;
        public readonly int ToX;
        public readonly int ToY;
        public readonly Chess.PieceType Type;
        public readonly Chess.PieceColor Color;
        public readonly PieceHeight Height;

        public RawMove(string designation, int x1, int y1, int x2, int y2)
        {
            FromX = x1;
            FromY = y1;
            ToX = x2;
            ToY = y2;

            string lcDes = designation.ToLower();
            Color = lcDes != designation ? Chess.PieceColor.White : Chess.PieceColor.Black;
            if (!pieceDesignations.ContainsKey(lcDes))
            {
                throw new Exception("Unknown piece designation '" + designation + "' in input data.");
            }
            Type = pieceDesignations[lcDes];
            Height = pieceHeights[Type];
        }

        public static bool isOut(int x, int y)
        {
            return x < 0 || x > 7 || y < 0 || y > 7;
        }

        public static string fieldName(int x, int y)
        {
            if (isOut(x, y))
            {
                return "discard";
            }

            return (char)((int)'a' + x) + (y + 1).ToString();
        }

        public override string ToString()
        {
            return Color.ToString() + " " + Type.ToString() + " " + fieldName(FromX, FromY) + " -> " + fieldName(ToX, ToY);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string configPath;
        private Config config = null;
        private ICraneDriver driver = null;
        private CraneChessManager chessManager = null;

        private string loadedFile = null;
        private List<RawMove> moves = null;
        private int selectedMove = -1;

        private bool playing = false;
        private bool closeRequest = false;
        private bool externalInput = false;
        private Queue<RawMove> externalMoves = new Queue<RawMove>();


        private static List<RawMove> LoadMoves(string fileName)
        {
            var moves = new List<RawMove>();
            IEnumerable<string> lines = File.ReadLines(fileName);
            foreach (var line in lines)
            {
                var tokens = line.Trim().Split(' ');
                if (tokens.Length != 5) continue;
                moves.Add(new RawMove(tokens[0], Int32.Parse(tokens[2]), Int32.Parse(tokens[1]), Int32.Parse(tokens[4]), Int32.Parse(tokens[3])));
            }
            return moves;
        }

        private Task pendingAction = null;
        private DispatcherTimer timer = new DispatcherTimer();

        private readonly TaskScheduler uiScheduler;

        private readonly FileSystemWatcher fsWatcher;
        private static readonly string externalInputFile = "../../input/move.cmm";

        public MainWindow()
        {
            InitializeComponent();

            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/../../config";
            Directory.CreateDirectory(dir);
            configPath = dir + "/ControlledApp.cfg";

            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            fsWatcher = new FileSystemWatcher(Path.GetDirectoryName(externalInputFile));
            fsWatcher.Filter = "*.cmm";
            fsWatcher.Created += OnInputFileCreated;
            fsWatcher.EnableRaisingEvents = true;

            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            UpdateControls();
        }

        private void TryGetExternalInput()
        {
            if (File.Exists(externalInputFile))
            {
                ListBoxMoves.Items.Add("Loading external input file...");
                externalMoves.Clear();
                var moves = LoadMoves(externalInputFile);
                foreach (var m in moves)
                {
                    externalMoves.Enqueue(m);
                    ListBoxMoves.Items.Add(m);
                }
                File.Delete(externalInputFile);
            }
        }
        private void OnInputFileCreated(object sender, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(300);
            this.Dispatcher.Invoke(() =>
            {
                this.TryGetExternalInput();
            });
        }

        private static readonly string LabelFloatFormat = "0.000";
        private static readonly System.Globalization.CultureInfo LabelFloatCult = System.Globalization.CultureInfo.InvariantCulture;

        private void UpdateControls()
        {
            CalibrationBtn.IsEnabled = chessManager != null && (moves == null || !playing) && !externalInput;
            LoadBtn.IsEnabled = chessManager != null && !externalInput;
            PlayBtn.IsEnabled = chessManager != null && moves != null && !playing && !externalInput;
            //PauseResumeBtn.IsEnabled = chessManager != null && moves != null;
            StopBtn.IsEnabled = chessManager != null && moves != null && playing;
            ResetBtn.IsEnabled = chessManager != null && moves != null && selectedMove != -1;
            CloseGripBtn.IsEnabled = chessManager != null && (moves == null || !playing) && !externalInput;
            ExternalInputBtn.IsEnabled = chessManager != null && !playing && !externalInput;
            StopExternalBtn.IsEnabled = chessManager != null && !playing && externalInput;

            LabelFileName.Content = loadedFile != null ? loadedFile : "";
            if (moves != null)
            {
                ListBoxMoves.Items.Clear();
                foreach (var move in moves)
                {
                    ListBoxMoves.Items.Add(move);
                }
                ListBoxMoves.SelectedIndex = selectedMove;
            }

            if (chessManager != null)
            {
                LabelActualX.Content = chessManager.Position.X.ToString(LabelFloatFormat, LabelFloatCult);
                LabelActualY.Content = chessManager.Position.Y.ToString(LabelFloatFormat, LabelFloatCult);
                LabelActualZ.Content = chessManager.Position.Z.ToString(LabelFloatFormat, LabelFloatCult);
            } else
            {
                LabelActualX.Content = LabelActualY.Content = LabelActualZ.Content = "-";
            }
        }

        private void SaveStateInConfig()
        {
            config.GameFile = loadedFile;
            config.MovesPlayed = selectedMove;
            config.SaveAs(configPath);
        }


        private async Task SelectPort()
        {
            var dialog = new PortSelectDialog();
            var res = dialog.QueryPort();
            if (res == null)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            try
            {
                if (res == "DUMMY")
                {
                    driver = new FakeCraneDriver();
                } else
                {
                    driver = null;
                    var newDriver = new CraneDriver(res);
                    await newDriver.Initialize();
                    driver = newDriver;
                }

                chessManager = new CraneChessManager(driver);
                if (config.GetCalibrationOrigin() != null && config.GetCalibrationFarpoint() != null)
                {
                    chessManager.Calibrate(config.GetCalibrationOrigin().Value, config.GetCalibrationFarpoint().Value);
                }
                
                
                LabelState.Content = "Initialized.";
                UpdateControls();
            }
            catch (Exception ex)
            {
                if (System.Windows.MessageBox.Show(ex.Message + "\nDo you wish to retry?", "Communication Error",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                    await SelectPort();
                else
                    System.Windows.Application.Current.Shutdown();
                return;
            }
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    config = Config.Load(configPath);
                }
                catch (Exception ex)
                {
                    if (System.Windows.MessageBox.Show(ex.Message + "\nDo you wish to replace config with defaults?", "Config File Invalid",
                       MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                    {
                        config = new Config();
                        config.SaveAs(configPath);
                    } else
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                    return;
                }
            } else
            {
                config = new Config();
                config.SaveAs(configPath);
            }

            await SelectPort();
        }

        private void finalizePendingAction()
        {
            if (pendingAction.Exception != null)
            {
                throw pendingAction.Exception;
            }
            UpdateControls();
            pendingAction = null;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (chessManager == null || pendingAction != null) return;

            // close request takes precedence
            if (closeRequest)
            {
                closeRequest = false;
                pendingAction = driver.CloseGrip().ContinueWith(_ => finalizePendingAction(), uiScheduler);
                return;
            }

            RawMove move = null;
            if (externalInput)
            {
                // external input handling
                if (externalMoves != null && externalMoves.Count > 0)
                {
                    move = externalMoves.Peek();
                }
            }
            else if (playing)
            {
                // get move from a scenario being replayed
                move = moves != null && selectedMove >= 0 && selectedMove < moves.Count ? moves[selectedMove] : null;
            }

            if (move == null) // all dressed up, nowhere to go...
            {
                if (externalInput)
                {
                    // in interactive mode, we need to home outside the chessboard so the kinect reading is not disrupted
                    var homingCoordinates = chessManager.GetCoordinates(4, 9); // two rows after row #8 (assuming, the user is playing for white)
                    float fromDist = (chessManager.PositionXY - homingCoordinates).Length();
                    if (fromDist > 1.0f)
                    {
                        pendingAction = chessManager.MoveTo(homingCoordinates).ContinueWith(_ => finalizePendingAction(), uiScheduler);
                        return;
                    }
                }
            }
            else
            {
                // perform selected move
                if (!chessManager.IsCarrying && !chessManager.IsMoving)
                {
                    float fromDist = (chessManager.PositionXY - chessManager.GetCoordinates(move.FromX, move.FromY)).Length();
                    if (fromDist > 1.0f)
                    {
                        pendingAction = chessManager.MoveToField(move.FromX, move.FromY).ContinueWith(_ => finalizePendingAction(), uiScheduler);
                        return;
                    }
                    else
                    {
                        pendingAction = chessManager.PickPiece(move.Height).ContinueWith(_ => finalizePendingAction(), uiScheduler);
                        return;
                    }
                }
                else if (chessManager.IsCarrying && !chessManager.IsMoving)
                {
                    float toDist = (chessManager.PositionXY - chessManager.GetCoordinates(move.ToX, move.ToY)).Length();
                    if (toDist > 1.0f)
                    {
                        pendingAction = chessManager.MoveToField(move.ToX, move.ToY).ContinueWith(_ => finalizePendingAction(), uiScheduler);
                        return;
                    }
                    else
                    {
                        pendingAction = chessManager.ReleasePiece().ContinueWith(_ =>
                        {
                            if (externalInput)
                            {
                                if (externalMoves != null && externalMoves.Count > 0)
                                {
                                    externalMoves.Dequeue();
                                }
                            }
                            else if (playing)
                            {
                                selectedMove = selectedMove + 1;
                                if (moves.Count <= selectedMove)
                                {
                                    playing = false;
                                }
                            }

                            finalizePendingAction();
                        }, uiScheduler);
                        return;
                    }
                }
            }

            // if no action takes precedence, tat least update the state...
            pendingAction = chessManager.UpdateState().ContinueWith(_ => finalizePendingAction(), uiScheduler);
        }


        private void CalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (chessManager != null)
            {
                CraneChessManager saveManager = chessManager;
                chessManager = null;
                UpdateControls();

                var dialog = new CalibrationForm();
                dialog.OpenCalibration(saveManager);
                
                config.CopyChessManagerState(saveManager);
                config.SaveAs(configPath);

                chessManager = saveManager;
                UpdateControls();
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.InitialDirectory = Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/../../../Data");
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    moves = LoadMoves(dlg.FileName);
                    loadedFile = dlg.FileName;
                    selectedMove = 0;
                    playing = false;
                    UpdateControls();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "File Load Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void ListBoxMoves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxMoves.SelectedIndex != selectedMove && ListBoxMoves.Items.Count > selectedMove && selectedMove >= -1)
            {
                ListBoxMoves.SelectedIndex = selectedMove;
            } else if (selectedMove == ListBoxMoves.Items.Count && ListBoxMoves.SelectedIndex != ListBoxMoves.Items.Count-1)
            {
                ListBoxMoves.SelectedIndex = ListBoxMoves.Items.Count - 1;
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            playing = true;
            if (selectedMove == -1)
            {
                selectedMove = 0;
            }
            UpdateControls();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            playing = false;
            UpdateControls();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            playing = false;
            selectedMove = -1;
            UpdateControls();
        }

        private void CloseGripBtn_Click(object sender, RoutedEventArgs e)
        {
            closeRequest = true;
        }

        private void ExternalInputBtn_Click(object sender, RoutedEventArgs e)
        {
            if (chessManager != null && !playing) {
                loadedFile = null;
                moves = null;
                selectedMove = -1;
                closeRequest = false;
                externalInput = true;
                
                UpdateControls();
                ListBoxMoves.Items.Clear();
                ListBoxMoves.Items.Add("External control...");
                this.TryGetExternalInput();
            }
        }

        private void StopExternalBtn_Click(object sender, RoutedEventArgs e)
        {
            if (chessManager != null && !playing)
            {
                externalInput = false;
                ListBoxMoves.Items.Clear();
                UpdateControls();
            }
        }
    }
}
