using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Numerics;

namespace ControllerApp
{
    /// <summary>
    /// Interaction logic for CalibrationForm.xaml
    /// </summary>
    public partial class CalibrationForm : Window
    {
        private static readonly Dictionary<Key, Vector2> keyUpdates = new Dictionary<Key, Vector2>();
        private Vector2 planeLimits = new Vector2(490, 800);

        private DispatcherTimer timer = new DispatcherTimer();

        private CraneChessManager chessManager = null;

        private bool homed = false;
        private bool pendingAction = false;
        private Task periodicUpdateTask = null;

        private Vector2 desired;
        private Vector2? lockedA1 = null;
        private Vector2? lockedH8 = null;
        private Vector2? previousA1 = null;
        private Vector2? previousH8 = null;

        private readonly Dictionary<Key, uint> keyStates = new Dictionary<Key, uint>();
        private bool shiftPressed = false;
        private bool ctrlPressed = false;

        private readonly TaskScheduler uiScheduler;
        public CalibrationForm()
        {
            InitializeComponent();

            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            keyUpdates[Key.Left] = new Vector2(-1f, 0f);
            keyUpdates[Key.Right] = new Vector2(1f, 0f);
            keyUpdates[Key.Up] = new Vector2(0f, -1f);
            keyUpdates[Key.Down] = new Vector2(0f, 1f);

            foreach (var key in keyUpdates.Keys) keyStates[key] = 0;

            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private static readonly string LabelFloatFormat = "0.000";
        private static readonly System.Globalization.CultureInfo LabelFloatCult = System.Globalization.CultureInfo.InvariantCulture;
        private void UpdateControls()
        {
            bool actionsEnabled = chessManager != null && !pendingAction;
            StopBtn.IsEnabled = actionsEnabled && chessManager.IsMoving;
            HomeBtn.IsEnabled = actionsEnabled && !chessManager.IsMoving && !chessManager.IsCarrying;

            LockA1Btn.IsEnabled = homed;
            LockH8Btn.IsEnabled = homed;
            UndoA1Btn.IsEnabled = previousA1 != null;
            UndoH8Btn.IsEnabled = previousH8 != null;

            bool testPickEnabled = actionsEnabled && homed && !chessManager.IsMoving;
            TestPickPawnBtn.IsEnabled = testPickEnabled && (!chessManager.IsCarrying || chessManager.CarriedPieceHeight == PieceHeight.Small);
            TestPickMiddleBtn.IsEnabled = testPickEnabled && (!chessManager.IsCarrying || chessManager.CarriedPieceHeight == PieceHeight.Medium);
            TestPickKingBtn.IsEnabled = testPickEnabled && (!chessManager.IsCarrying || chessManager.CarriedPieceHeight == PieceHeight.High);

            if (chessManager != null)
            {
                TestPickPawnBtn.Content = chessManager.IsCarrying && chessManager.CarriedPieceHeight == PieceHeight.Small ? "Release Pawn" : "Pick Pawn";
                TestPickMiddleBtn.Content = chessManager.IsCarrying && chessManager.CarriedPieceHeight == PieceHeight.Medium ? "Release B/K/R" : "Pick B/K/R";
                TestPickKingBtn.Content = chessManager.IsCarrying && chessManager.CarriedPieceHeight == PieceHeight.High ? "Release King" : "Pick King";
            }

            bool testMoveEnabled = actionsEnabled && homed && chessManager.IsCalibrated;
            TestH1Btn.IsEnabled = testMoveEnabled;
            TestA8Btn.IsEnabled = testMoveEnabled;
            TestE4Btn.IsEnabled = testMoveEnabled;

            // labels
            LabelDesiredX.Content = homed ? desired.X.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelDesiredY.Content = homed ? desired.Y.ToString(LabelFloatFormat, LabelFloatCult) : "-";

            LabelActualX.Content = chessManager != null ? chessManager.Position.X.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelActualY.Content = chessManager != null ? chessManager.Position.Y.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelActualZ.Content = chessManager != null ? chessManager.Position.Z.ToString(LabelFloatFormat, LabelFloatCult) : "-";

            LabelA1X.Content = lockedA1 != null ? lockedA1.Value.X.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelA1Y.Content = lockedA1 != null ? lockedA1.Value.Y.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelH8X.Content = lockedH8 != null ? lockedH8.Value.X.ToString(LabelFloatFormat, LabelFloatCult) : "-";
            LabelH8Y.Content = lockedH8 != null ? lockedH8.Value.Y.ToString(LabelFloatFormat, LabelFloatCult) : "-";

            if (chessManager == null)
            {
                LabelState.Content = "Disconnected";
            } else if (!homed) {
                LabelState.Content = "Homing required!";
            } else if (chessManager.IsMoving)
            {
                LabelState.Content = chessManager.IsCarrying ? "Carrying" : "Moving";
            } else
            {
                LabelState.Content = chessManager.IsCarrying ? "Holding" : "Idle";
            }
        }

        private void UpdateDesiredPosition(Key key)
        {
            if (keyUpdates.ContainsKey(key))
            {
                Vector2 update = keyUpdates[key];
                if (!shiftPressed) {
                    update *= ctrlPressed ? 100.0f : 10.0f;
                }
                desired += update;
                desired.X = Math.Max(Math.Min(desired.X, planeLimits.X), 0f);
                desired.Y = Math.Max(Math.Min(desired.Y, planeLimits.Y), 0f);
                
                UpdateControls();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (chessManager == null) return;

            bool anyKeyPressed = false;
            foreach (var key in keyUpdates.Keys)
            {
                if (keyStates[key] > 0)
                {
                    keyStates[key] += 1;
                    anyKeyPressed = true;
                }
                if (keyStates[key] > 3)
                {
                    UpdateDesiredPosition(key);
                }
            }

            if (!pendingAction && periodicUpdateTask == null)
            {
                float dx = desired.X - chessManager.Position.X;
                float dy = desired.Y - chessManager.Position.Y;
                bool notAtDesired = Math.Abs(dx) > 0.5 || Math.Abs(dy) > 0.5;
                bool quiteFar = Math.Sqrt(dx * dx + dy * dy) > 100f;
                
                if (homed && !chessManager.IsMoving && !chessManager.IsPaused && (quiteFar || (notAtDesired && !anyKeyPressed)))
                {
                    // start movement (which also updates the internal state)
                    periodicUpdateTask = chessManager.MoveTo(desired).ContinueWith(_ =>
                    {
                        if (periodicUpdateTask.Exception != null)
                        {
                            throw periodicUpdateTask.Exception;
                        }
                        UpdateControls();
                        periodicUpdateTask = null;
                    }, uiScheduler);
                } else
                {
                    // periodic update of crane state
                    periodicUpdateTask = chessManager.UpdateState().ContinueWith(_ =>
                    {
                        if (periodicUpdateTask.Exception != null)
                        {
                            throw periodicUpdateTask.Exception;
                        }
                        UpdateControls();
                        periodicUpdateTask = null;
                    }, uiScheduler);
                }
            }
        }

        private void CalibrateChessManager()
        {
            if (chessManager != null && lockedA1 != null && lockedH8 != null)
            {
                chessManager.Calibrate(lockedA1.Value, lockedH8.Value);
            }
        }

        private async Task StartAction()
        {
            if (pendingAction) return;

            pendingAction = true;
            UpdateControls();

            if (periodicUpdateTask != null)
            {
                await periodicUpdateTask;
                await Task.Delay(50);
            }
        }
        // [182] = "< X492.0 Y814.8 Z90.0"
        public void OpenCalibration(CraneChessManager manager)
        {
            chessManager = manager;
            planeLimits = new Vector2(chessManager.Limits.X, chessManager.Limits.Y);
            if (chessManager.IsCalibrated)
            {
                lockedA1 = chessManager.Origin;
                lockedH8 = chessManager.Farpoint;
            } else
            {
                lockedA1 = null;
                lockedH8 = null;
            }

            UpdateControls();
            ShowDialog();
            this.chessManager = null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (keyStates.ContainsKey(e.Key))
            {
                keyStates[e.Key] = 1;
                UpdateDesiredPosition(e.Key);
            }
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = true;
            }
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlPressed = true;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (keyStates.ContainsKey(e.Key))
            {
                keyStates[e.Key] = 0;
            }
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = false;
            }
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctrlPressed = false;
            }
        }

        private async void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pendingAction) return;

            await StartAction();
            LabelState.Content = "Homing...";
            await chessManager.Home();

            homed = true;
            desired = chessManager.PositionXY;
            pendingAction = false;
            UpdateControls();
        }

        private async void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pendingAction) return;

            await StartAction();
            await chessManager.Stop();

            desired = chessManager.PositionXY;
            pendingAction = false;
            UpdateControls();
        }

        private void LockA1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (homed)
            {
                if (lockedA1 != null)
                {
                    previousA1 = lockedA1;
                }
                lockedA1 = chessManager.PositionXY;
                CalibrateChessManager();
                UpdateControls();
            }
        }

        private void LockH8Btn_Click(object sender, RoutedEventArgs e)
        {
            if (homed)
            {
                if (lockedH8 != null)
                {
                    previousH8 = lockedH8;
                }
                lockedH8 = chessManager.PositionXY;
                CalibrateChessManager();
                UpdateControls();
            }
        }

        private void UndoA1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (previousA1 != null)
            {
                var save = lockedA1;
                lockedA1 = previousA1;
                previousA1 = save;
                CalibrateChessManager();
                UpdateControls();
            }
        }

        private void UndoH8Btn_Click(object sender, RoutedEventArgs e)
        {
            if (previousH8 != null)
            {
                var save = lockedH8;
                lockedH8 = previousH8;
                previousH8 = save;
                CalibrateChessManager();
                UpdateControls();
            }
        }

        private void TestMove(int col, int row)
        {
            if (chessManager == null || !homed || pendingAction || !chessManager.IsCalibrated) return;

            desired = chessManager.GetCoordinates(col, row);
            if (desired.X < 0f || desired.X > planeLimits.X || desired.Y < 0f || desired.Y > planeLimits.Y)
            {
                // invalid move !!!
                chessManager.InvalidateCalibration();
                desired = chessManager.PositionXY;
            }
            else
            {
                foreach (var key in keyUpdates.Keys) keyStates[key] = 0;
            }
            UpdateControls();
        }

        private void TestH1Btn_Click(object sender, RoutedEventArgs e)
        {
            TestMove(7, 0);
        }

        private void TestA8Btn_Click(object sender, RoutedEventArgs e)
        {
            TestMove(0, 7);
        }

        private void TestE4Btn_Click(object sender, RoutedEventArgs e)
        {
            TestMove(4, 3);
        }

        private async Task PickFigure(PieceHeight height)
        {
            if (chessManager == null || !homed || pendingAction || chessManager.IsMoving) return;
            
            await StartAction();
            if (chessManager.IsCarrying)
            {
                await chessManager.ReleasePiece();
            }
            else
            {
                await chessManager.PickPiece(height);
            }

            pendingAction = false;
            UpdateControls();
        }

        private async void TestPickPawnBtn_Click(object sender, RoutedEventArgs e)
        {
            await PickFigure(PieceHeight.Small);
        }

        private async void TestPickMiddleBtn_Click(object sender, RoutedEventArgs e)
        {
            await PickFigure(PieceHeight.Medium);
        }

        private async void TestPickKingBtn_Click(object sender, RoutedEventArgs e)
        {
            await PickFigure(PieceHeight.High);
        }

        private void AcknowledgeHomingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!homed && !pendingAction)
            {
                homed = true;
                desired = chessManager.PositionXY;
                UpdateControls();
            }
        }

        private void MoveA1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (chessManager != null && !pendingAction && homed && lockedA1 != null)
            {
                desired = lockedA1.Value;
            }
        }

        private void MoveH8Btn_Click(object sender, RoutedEventArgs e)
        {
            if (chessManager != null && !pendingAction && homed && lockedH8 != null)
            {
                desired = lockedH8.Value;
            }
        }
    }
}
