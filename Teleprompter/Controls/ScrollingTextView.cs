using System.Diagnostics;
using Microsoft.Maui.Animations;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Teleprompter.Services;

namespace Teleprompter.Controls
{
    public class ScrollingTextView : ContentView
    {
        private readonly IScrollEngine _engine = new ScrollEngine();
        private readonly SKCanvasView _canvas;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _highlightPaint;
        private readonly SKFont _font;
        private readonly SKTypeface? _typeface;

        private Microsoft.Maui.Animations.Animation? _scrollAnimation;
        private IDispatcherTimer? _fallbackTicker;

        private string _fullText = string.Empty;
        private double _viewHeight;
        private double _viewWidth;
        private double _scrollOffset;
        private bool _contentReady;
        private bool _endReached;
        private TextLayout? _layout;
        private int _activeWordIndex = -1;
        private int _layoutGeneration;
        private DateTime _lastProgressRaiseTime = DateTime.MinValue;

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(ScrollingTextView), string.Empty,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyText());

        public static readonly BindableProperty SpeedProperty =
            BindableProperty.Create(nameof(Speed), typeof(double), typeof(ScrollingTextView), 60.0,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).OnSpeedChanged());

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ScrollingTextView), 28.0,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyFontSize());

        public static readonly BindableProperty MirrorModeProperty =
            BindableProperty.Create(nameof(MirrorMode), typeof(bool), typeof(ScrollingTextView), false,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyMirrorMode());

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ScrollingTextView), Colors.White,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyTextColor());

        public static readonly BindableProperty HighlightEnabledProperty =
            BindableProperty.Create(nameof(HighlightEnabled), typeof(bool), typeof(ScrollingTextView), false,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyHighlightEnabled());

        public static readonly BindableProperty HighlightWpmProperty =
            BindableProperty.Create(nameof(HighlightWpm), typeof(double), typeof(ScrollingTextView), 180.0,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyHighlight());

        public static readonly BindableProperty HighlightColorProperty =
            BindableProperty.Create(nameof(HighlightColor), typeof(Color), typeof(ScrollingTextView), Colors.Yellow,
                propertyChanged: (b, _, _) => ((ScrollingTextView)b).ApplyHighlightColor());

        public event EventHandler<double>? ProgressChanged;
        public event EventHandler<bool>? IsPlayingChanged;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double Speed
        {
            get => (double)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public bool MirrorMode
        {
            get => (bool)GetValue(MirrorModeProperty);
            set => SetValue(MirrorModeProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public bool HighlightEnabled
        {
            get => (bool)GetValue(HighlightEnabledProperty);
            set => SetValue(HighlightEnabledProperty, value);
        }

        public double HighlightWpm
        {
            get => (double)GetValue(HighlightWpmProperty);
            set => SetValue(HighlightWpmProperty, value);
        }

        public Color HighlightColor
        {
            get => (Color)GetValue(HighlightColorProperty);
            set => SetValue(HighlightColorProperty, value);
        }

        public bool IsPlaying => _engine.IsPlaying;

        public double Progress { get; private set; }

        public ScrollingTextView()
        {
            _typeface = CreateTypeface();
            _font = new SKFont { Typeface = _typeface ?? SKTypeface.Default, Size = (float)FontSize };
            _textPaint = new SKPaint { IsAntialias = true };
            _highlightPaint = new SKPaint { IsAntialias = true };

            _canvas = new SKCanvasView();
            _canvas.PaintSurface += OnPaintSurface;
            Content = _canvas;

            SizeChanged += OnViewSizeChanged;

            ApplyTextColor();
            ApplyHighlightColor();
            RebuildLayoutAsync();
        }

        public void Play()
        {
            if (Progress >= 1)
                _engine.Stop();

            if (_engine.IsPlaying)
                return;

            _endReached = false;
            _engine.Play();
            StartScrolling();
            IsPlayingChanged?.Invoke(this, true);
        }

        public void Pause()
        {
            if (!_engine.IsPlaying)
                return;

            _engine.Pause();
            StopScrolling();
            UpdatePosition(CurrentPixels());
            ApplyHighlight();
            IsPlayingChanged?.Invoke(this, false);
        }

        public void Stop()
        {
            _engine.Stop();
            StopScrolling();
            _endReached = false;
            UpdatePosition(0);
            ApplyHighlight();
            IsPlayingChanged?.Invoke(this, false);
        }

        public void SeekBy(double seconds)
        {
            _engine.SeekBy(seconds);
            _endReached = false;

            if (_engine.IsPlaying)
            {
                StartScrolling();
            }
            else
            {
                UpdatePosition(CurrentPixels());
                ApplyHighlight();
            }
        }

        private void OnSpeedChanged()
        {
            if (_engine.IsPlaying)
                StartScrolling();
        }

        private void ApplyText()
        {
            _fullText = Text ?? string.Empty;
            _contentReady = false;
            _layout = null;
            _activeWordIndex = -1;
            _endReached = false;
            _scrollOffset = 0;
            RebuildLayoutAsync();
        }

        private void ApplyFontSize()
        {
            _font.Size = (float)FontSize;
            RebuildLayoutAsync();
        }

        private void ApplyMirrorMode() => _canvas.InvalidateSurface();

        private void ApplyTextColor()
        {
            _textPaint.Color = ToSkColor(TextColor);
            _canvas.InvalidateSurface();
        }

        private void ApplyHighlightEnabled()
        {
            _activeWordIndex = -1;
            ApplyHighlight();
        }

        private void ApplyHighlightColor()
        {
            _highlightPaint.Color = ToSkColor(HighlightColor);
            _canvas.InvalidateSurface();
        }

        private void ApplyHighlight()
        {
            var index = -1;

            if (HighlightEnabled && _layout is not null && _layout.Words.Count > 0)
            {
                var secondsPerWord = 60.0 / Math.Max(1, HighlightWpm);
                index = (int)(_engine.ElapsedSeconds / secondsPerWord);
                index = Math.Clamp(index, 0, _layout.Words.Count - 1);
            }

            if (index == _activeWordIndex)
                return;

            _activeWordIndex = index;
            _canvas.InvalidateSurface();
        }

        private void OnViewSizeChanged(object? sender, EventArgs e)
        {
            try
            {
                var oldWidth = _viewWidth;
                _viewHeight = Height;
                _viewWidth = Width;

                if (double.IsNaN(_viewHeight) || double.IsNaN(_viewWidth))
                    return;

                if (_fullText.Length > 0 && Math.Abs(oldWidth - _viewWidth) > 0.5)
                    RebuildLayoutAsync();
                else
                    _canvas.InvalidateSurface();

                if (!_engine.IsPlaying)
                    UpdatePosition(CurrentPixels());
            }
            catch (Exception ex)
            {
                LogError("Size changed handler failed", ex);
            }
        }

        private double CurrentPixels() => _engine.ElapsedSeconds * Speed;

        private double GetTotalDistance() => (_layout?.ContentHeight ?? 0) + _viewHeight;

        private double GetEndPixels() => GetTotalDistance();

        private void StartScrolling()
        {
            StopScrolling();

            if (!_contentReady)
                return;

            var totalDistance = GetTotalDistance();
            var start = CurrentPixels();
            if (start >= totalDistance)
            {
                FinishScrolling();
                return;
            }

            var remainingSeconds = (totalDistance - start) / Math.Max(1, Speed);

            var manager = Handler?.MauiContext?.Services.GetService<IAnimationManager>();
            if (manager is not null)
            {
                _scrollAnimation = new Microsoft.Maui.Animations.Animation(
                    progress =>
                    {
                        try
                        {
                            var pixels = start + (totalDistance - start) * progress;
                            UpdatePosition(pixels);
                            ApplyHighlight();
                        }
                        catch (Exception ex)
                        {
                            LogError("Animation tick failed", ex);
                        }
                    },
                    0,
                    remainingSeconds,
                    Easing.Linear,
                    FinishScrolling);

                _scrollAnimation.Commit(manager);
                return;
            }

            StartFallbackTicker();
        }

        private void StartFallbackTicker()
        {
            if (_fallbackTicker is null)
            {
                _fallbackTicker = Dispatcher.CreateTimer();
                _fallbackTicker.Interval = TimeSpan.FromMilliseconds(16);
                _fallbackTicker.Tick += (_, _) =>
                {
                    try
                    {
                        if (!_engine.IsPlaying)
                        {
                            _fallbackTicker?.Stop();
                            return;
                        }

                        UpdatePosition(CurrentPixels());
                        ApplyHighlight();
                    }
                    catch (Exception ex)
                    {
                        LogError("Ticker tick failed", ex);
                    }
                };
            }

            _fallbackTicker.Start();
        }

        private void StopScrolling()
        {
            _scrollAnimation?.Pause();
            _scrollAnimation = null;
            _fallbackTicker?.Stop();
        }

        private void FinishScrolling()
        {
            _scrollAnimation = null;
            _fallbackTicker?.Stop();
            _engine.Pause();
            UpdatePosition(GetEndPixels());
            ApplyHighlight();
            IsPlayingChanged?.Invoke(this, false);
        }

        private void UpdatePosition(double absolutePixel)
        {
            _scrollOffset = absolutePixel;

            if (!_contentReady)
            {
                SetProgress(0);
                _canvas.InvalidateSurface();
                return;
            }

            var totalDistance = GetTotalDistance();
            if (_scrollOffset >= totalDistance && totalDistance > 0)
            {
                _scrollOffset = totalDistance;
                if (!_endReached)
                {
                    _endReached = true;
                    _engine.Pause();
                    StopScrolling();
                    IsPlayingChanged?.Invoke(this, false);
                }
            }

            var progress = Math.Clamp(_scrollOffset / Math.Max(1, totalDistance), 0, 1);
            SetProgress(progress);
            _canvas.InvalidateSurface();
        }

        private void SetProgress(double value)
        {
            var delta = Math.Abs(Progress - value);
            var enoughTime = (DateTime.UtcNow - _lastProgressRaiseTime).TotalMilliseconds >= 100;

            if (delta < 0.001 && !enoughTime)
                return;

            Progress = value;
            _lastProgressRaiseTime = DateTime.UtcNow;
            ProgressChanged?.Invoke(this, value);
        }

        private async void RebuildLayoutAsync()
        {
            var generation = ++_layoutGeneration;
            var text = _fullText;
            var fontSize = (float)FontSize;
            var width = (float)Math.Max(1, _viewWidth);

            TextLayout? layout;
            try
            {
                layout = await Task.Run(() => ComputeLayout(text, fontSize, width));
            }
            catch (Exception ex)
            {
                LogError("Layout compute failed", ex);
                return;
            }

            if (layout is null || generation != _layoutGeneration)
                return;

            ApplyLayout(layout);
        }

        private void ApplyLayout(TextLayout layout)
        {
            var dispatcher = Dispatcher;
            if (dispatcher is not null && dispatcher.IsDispatchRequired)
                dispatcher.Dispatch(() => ApplyLayoutOnUi(layout));
            else
                ApplyLayoutOnUi(layout);
        }

        private void ApplyLayoutOnUi(TextLayout layout)
        {
            try
            {
                _layout = layout;
                _contentReady = layout.Words.Count > 0 && _viewHeight > 0;
                _activeWordIndex = -1;

                if (_engine.IsPlaying)
                    StartScrolling();
                else
                    UpdatePosition(CurrentPixels());
            }
            catch (Exception ex)
            {
                LogError("Layout apply failed", ex);
            }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            try
            {
                PaintSurfaceCore(e);
            }
            catch (Exception ex)
            {
                LogError("Paint failed", ex);
            }
        }

        private void PaintSurfaceCore(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_layout is null || _layout.Words.Count == 0)
                return;

            if (e.Info.Width <= 0 || _viewHeight <= 0)
                return;

            var viewWidthDip = (float)Math.Max(1, Width);
            var density = e.Info.Width / viewWidthDip;
            if (density <= 0)
                return;

            canvas.Save();
            canvas.Scale(density, density);

            if (MirrorMode)
            {
                canvas.Translate(viewWidthDip, 0);
                canvas.Scale(-1, 1);
            }

            DrawText(canvas);

            canvas.Restore();
        }

        private void DrawText(SKCanvas canvas)
        {
            var layout = _layout!;
            if (layout.LineTops.Length == 0 || layout.LineWordStart.Length == 0)
                return;

            var metrics = _font.Metrics;
            var offset = (float)_scrollOffset;
            var viewH = (float)_viewHeight;

            var first = LowerBound(layout.LineTops, offset - viewH);
            var last = UpperBound(layout.LineTops, offset) - 1;

            for (var l = first; l <= last && l < layout.LineTops.Length; l++)
            {
                var lineY = layout.LineTops[l] + viewH - offset;
                var baseline = lineY - metrics.Ascent;
                var wStart = layout.LineWordStart[l];
                var wEnd = (l + 1 < layout.LineWordStart.Length) ? layout.LineWordStart[l + 1] : layout.Words.Count;

                for (var k = wStart; k < wEnd; k++)
                {
                    var word = layout.Words[k];
                    var paint = (HighlightEnabled && k == _activeWordIndex) ? _highlightPaint : _textPaint;
                    canvas.DrawText(word.Text, word.X, baseline, SKTextAlign.Left, _font, paint);
                }
            }
        }

        private static int LowerBound(IReadOnlyList<float> values, float target)
        {
            var lo = 0;
            var hi = values.Count;

            while (lo < hi)
            {
                var mid = (lo + hi) >> 1;
                if (values[mid] < target)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lo;
        }

        private static int UpperBound(IReadOnlyList<float> values, float target)
        {
            var lo = 0;
            var hi = values.Count;

            while (lo < hi)
            {
                var mid = (lo + hi) >> 1;
                if (values[mid] <= target)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            return lo;
        }

        private static SKColor ToSkColor(Color color) =>
            new(
                (byte)Math.Round(color.Red * 255),
                (byte)Math.Round(color.Green * 255),
                (byte)Math.Round(color.Blue * 255),
                (byte)Math.Round(color.Alpha * 255));

        private static void LogError(string context, Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("Teleprompter", $"{context}: {ex}");
#else
            Debug.WriteLine($"[Teleprompter] {context}: {ex}");
#endif
        }

        private static SKTypeface CreateTypeface()
        {
#if ANDROID
            return SKTypeface.FromFamilyName("sans-serif") ?? SKTypeface.Default;
#elif IOS || MACCATALYST
            return SKTypeface.FromFamilyName("Helvetica Neue") ?? SKTypeface.Default;
#elif WINDOWS
            return SKTypeface.FromFamilyName("Segoe UI") ?? SKTypeface.Default;
#else
            return SKTypeface.Default;
#endif
        }

        private static TextLayout ComputeLayout(string text, float fontSize, float maxWidth)
        {
            if (maxWidth <= 0)
                maxWidth = 1;

            var typeface = CreateTypeface();

            try
            {
                var paint = new SKPaint { IsAntialias = true };
                var font = new SKFont { Typeface = typeface ?? SKTypeface.Default, Size = fontSize };

                try
                {
                    var metrics = font.Metrics;
                    var lineHeight = (metrics.Descent - metrics.Ascent) * 1.1f;
                    if (lineHeight <= 0)
                        lineHeight = fontSize;

                    var spaceWidth = font.MeasureText(" ", paint);

                    var words = new List<WordLayout>();
                    var lineTops = new List<float>();
                    var lineWordStart = new List<int>();

                    float x = 0;
                    float lineTop = 0;
                    var currentLineFirstWord = -1;

                    foreach (var token in Tokenize(text))
                    {
                        if (token.Kind == TokenKind.Newline)
                        {
                            if (currentLineFirstWord >= 0)
                            {
                                currentLineFirstWord = -1;
                                x = 0;
                            }

                            lineTop += lineHeight;
                            continue;
                        }

                        var wordWidth = font.MeasureText(token.Text, paint);
                        if (currentLineFirstWord >= 0 && x + wordWidth > maxWidth)
                        {
                            lineTop += lineHeight;
                            x = 0;
                            currentLineFirstWord = -1;
                        }

                        if (currentLineFirstWord < 0)
                        {
                            currentLineFirstWord = words.Count;
                            lineWordStart.Add(currentLineFirstWord);
                            lineTops.Add(lineTop);
                        }
                        else
                        {
                            x += spaceWidth;
                        }

                        words.Add(new WordLayout(token.Text, x, lineTop, token.Start));
                        x += wordWidth;
                    }

                    var contentHeight = currentLineFirstWord >= 0 ? lineTop + lineHeight : lineTop;

                    return new TextLayout
                    {
                        Words = words,
                        LineTops = lineTops.ToArray(),
                        LineWordStart = lineWordStart.ToArray(),
                        ContentHeight = contentHeight,
                    };
                }
                finally
                {
                    font.Dispose();
                    paint.Dispose();
                }
            }
            finally
            {
                if (typeface is not null && !ReferenceEquals(typeface, SKTypeface.Default))
                    typeface.Dispose();
            }
        }

        private static List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var n = text.Length;
            var i = 0;

            while (i < n)
            {
                var c = text[i];

                if (c == '\n' || c == '\r')
                {
                    var start = i;
                    if (c == '\r' && i + 1 < n && text[i + 1] == '\n')
                        i += 2;
                    else
                        i++;

                    tokens.Add(new Token(TokenKind.Newline, string.Empty, start));
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                var wordStart = i;
                while (i < n && !char.IsWhiteSpace(text[i]))
                    i++;

                tokens.Add(new Token(TokenKind.Word, text.Substring(wordStart, i - wordStart), wordStart));
            }

            return tokens;
        }

        private enum TokenKind
        {
            Word,
            Newline,
        }

        private readonly struct Token
        {
            public readonly TokenKind Kind;
            public readonly string Text;
            public readonly int Start;

            public Token(TokenKind kind, string text, int start)
            {
                Kind = kind;
                Text = text;
                Start = start;
            }
        }

        private readonly struct WordLayout
        {
            public readonly string Text;
            public readonly float X;
            public readonly float LineTop;
            public readonly int CharStart;

            public WordLayout(string text, float x, float lineTop, int charStart)
            {
                Text = text;
                X = x;
                LineTop = lineTop;
                CharStart = charStart;
            }
        }

        private sealed class TextLayout
        {
            public List<WordLayout> Words { get; init; } = new();
            public float[] LineTops { get; init; } = Array.Empty<float>();
            public int[] LineWordStart { get; init; } = Array.Empty<int>();
            public float ContentHeight { get; init; }
        }
    }
}
