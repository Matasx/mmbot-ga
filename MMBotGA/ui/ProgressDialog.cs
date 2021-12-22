using Downloader.Core.Core;
using Terminal.Gui;

namespace MMBotGA.ui
{
    internal class ProgressDialog : IProgress
    {
        private readonly Window _window;
        private readonly Dialog _dialog;
        private readonly Label _label;
        private readonly ProgressBar _progressBar;
        private bool _visible;
        private readonly object _lock = new();
        private readonly ColorScheme _baseColor;
        private readonly ColorScheme _errorColor;

        public ProgressDialog(Window window)
        {
            _window = window;
            _dialog = new Dialog("Downloading ...")
            {
                Modal = true
            };
            _label = new Label
            {
                X = Pos.Center(),
                Y = Pos.Center()
            };
            _progressBar = new ProgressBar
            {
                Y = Pos.Bottom(_label),
                Width = Dim.Fill(),
                Height = 1
            };
            _baseColor = _progressBar.ColorScheme;
            _errorColor = new ColorScheme()
            {
                Normal = Attribute.Make(Color.Red, _baseColor.Normal.Background)
            };

            _dialog.Add(_label, _progressBar);
        }

        public void Report(string name, int current, int total, bool? error)
        {
            Application.MainLoop.Invoke(() =>
            {
                var percentage = (float) current / total;
                _label.Text = $"{name} { (int)(percentage * 100)} %";
                _progressBar.Fraction = percentage;
                if (error.HasValue)
                {
                    _progressBar.ColorScheme = error.Value ? _errorColor : _baseColor;
                }
            });
            Show();
        }

        public void Show()
        {
            lock (_lock)
            {
                if (!_visible)
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        _window.Add(_dialog);
                    });
                    _visible = true;
                }
            }
        }

        public void Hide()
        {
            lock (_lock)
            {
                if (_visible)
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        _window.Remove(_dialog);
                    });
                    _visible = false;
                }
            }
        }
    }
}