# پیشنهادات Refactoring برای پنجره‌های LoopWindowTest

**تاریخ:** 2025-10-20  
**موضوع:** تفکیک منطق رسم از پنجره‌های WPF

---

## تحلیل کد فعلی

### بخش‌های مشترک پنجره‌های Test 1 تا 3

#### ۱. ساختار اصلی رندرینگ (یکسان ۱۰۰٪)
- `RenderTargetBitmap` - بیت‌مپ هدف برای رسم
- `DrawingVisual` و `DrawingContext` - ابزارهای رسم
- `DispatcherTimer` - حلقه تایمر برای رندر
- تنظیمات: `fps`, `screenWidth`, `screenHeight`
- متد `DispatcherTimer_Tick` - حلقه اصلی فریم

#### ۲. سیستم FPS (یکسان ۱۰۰٪)
- `lastFrameTime`, `currentFps`, `fpsHistory`, `fpsHistorySize`
- محاسبه deltaTime
- میانگین‌گیری FPS در هر فریم

#### ۳. پترن رندرینگ (یکسان ۱۰۰٪)
```csharp
GraphicScreen() → DrawingVisual
renderBitmap.Clear()
renderBitmap.Render(DVisuals)
```

#### ۴. رسم پس‌زمینه
- رسم background noise
- رسم شمارنده FPS

#### ۵. بخش‌های متفاوت
- منطق رسم اختصاصی (مستطیل‌ها / مکعب)
- متغیرهای انیمیشن (زوایا، سرعت‌ها)
- متدهای کمکی (`RectiPecti`, `DrawCube`, `RotatePoint`, ...)

---

## روش‌های پیشنهادی Refactoring

### روش ۱: Interface-based Architecture ✅ (پیشنهاد اصلی)

**مفهوم:** تفکیک کامل منطق رسم از پنجره با استفاده از Interface

#### ساختار:

```csharp
// Interface for drawing logic
public interface ISceneRenderer
{
    void Initialize(int width, int height);
    void Update(double deltaTime);
    void Draw(DrawingContext context, int width, int height);
    int ScreenWidth { get; }
    int ScreenHeight { get; }
}

// Base generic render window
public class BaseRenderWindow : Window
{
    private ISceneRenderer sceneRenderer;
    private RenderTargetBitmap renderBitmap;
    private DateTime lastFrameTime = DateTime.Now;
    private double currentFps = 0;
    private Queue<double> fpsHistory = new Queue<double>();
    private const int fpsHistorySize = 30;
    
    public BaseRenderWindow(ISceneRenderer renderer)
    {
        sceneRenderer = renderer;
        
        // Initialize the render target bitmap
        renderBitmap = new RenderTargetBitmap(
            renderer.ScreenWidth, 
            renderer.ScreenHeight, 
            96, 96, 
            PixelFormats.Pbgra32);
        
        // Setup timer
        DispatcherTimer timer = new DispatcherTimer();
        timer.Tick += DispatcherTimer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
        timer.Start();
        
        sceneRenderer.Initialize(renderer.ScreenWidth, renderer.ScreenHeight);
    }
    
    private void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        // Calculate FPS
        DateTime currentTime = DateTime.Now;
        double deltaTime = (currentTime - lastFrameTime).TotalSeconds;
        lastFrameTime = currentTime;
        
        if (deltaTime > 0)
        {
            double instantFps = 1.0 / deltaTime;
            fpsHistory.Enqueue(instantFps);
            
            if (fpsHistory.Count > fpsHistorySize)
                fpsHistory.Dequeue();
            
            currentFps = fpsHistory.Average();
        }
        
        // Update scene
        sceneRenderer.Update(deltaTime);
        
        // Render
        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            sceneRenderer.Draw(dc, sceneRenderer.ScreenWidth, sceneRenderer.ScreenHeight);
            CommonCustomDrawing.DrawFpsCounter(dc, currentFps, this, 
                sceneRenderer.ScreenWidth, sceneRenderer.ScreenHeight);
        }
        
        renderBitmap.Clear();
        renderBitmap.Render(visual);
    }
}

// Specific implementation example
public class CubeSceneRenderer : ISceneRenderer
{
    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }
    
    private double rotationAngleX = 0;
    private double rotationAngleY = 0;
    private double rotationAngleZ = 0;
    private double rotationSpeed = 30;
    
    public void Initialize(int width, int height)
    {
        ScreenWidth = width;
        ScreenHeight = height;
    }
    
    public void Update(double deltaTime)
    {
        rotationAngleX += rotationSpeed * deltaTime;
        rotationAngleY += rotationSpeed * deltaTime * 0.7;
        rotationAngleZ += rotationSpeed * deltaTime * 0.5;
        
        if (rotationAngleX >= 360) rotationAngleX -= 360;
        if (rotationAngleY >= 360) rotationAngleY -= 360;
        if (rotationAngleZ >= 360) rotationAngleZ -= 360;
    }
    
    public void Draw(DrawingContext dc, int width, int height)
    {
        // Draw background noise
        BitmapSource noise = NoiseMethods.UniformRandomNoiseImage(width, height, 12);
        dc.DrawImage(noise, new Rect(0, 0, width, height));
        
        // Draw cube
        DrawCube(dc, width, height);
    }
    
    private void DrawCube(DrawingContext dc, int width, int height)
    {
        // ... cube drawing logic ...
    }
}
```

#### استفاده:

```csharp
// In MainWindow or startup
var cubeRenderer = new CubeSceneRenderer();
var window = new BaseRenderWindow(cubeRenderer);
window.Show();
```

#### مزایا:
- ✅ تفکیک کامل منطق رسم از پنجره
- ✅ قابلیت استفاده مجدد بالا
- ✅ آزمایش‌پذیری (testability) عالی
- ✅ می‌توانید منطق رسم را در runtime عوض کنید
- ✅ Single Responsibility Principle
- ✅ Dependency Injection friendly

#### معایب:
- ⚠️ نیاز به کد بیشتر در ابتدا
- ⚠️ انتزاع بیشتر (ممکن است برای پروژه‌های کوچک زیادی باشد)

---

### روش ۲: Abstract Base Class

**مفهوم:** استفاده از کلاس پایه انتزاعی برای کدهای مشترک

```csharp
public abstract class BaseLoopWindow : Window
{
    protected RenderTargetBitmap renderBitmap;
    protected double currentFps = 0;
    private DateTime lastFrameTime = DateTime.Now;
    private Queue<double> fpsHistory = new Queue<double>();
    private const int fpsHistorySize = 30;
    
    protected abstract int ScreenWidth { get; }
    protected abstract int ScreenHeight { get; }
    protected abstract int TargetFps { get; }
    
    public BaseLoopWindow()
    {
        InitializeComponent();
        InitializeRenderingEngine();
    }
    
    private void InitializeRenderingEngine()
    {
        renderBitmap = new RenderTargetBitmap(
            ScreenWidth, ScreenHeight, 96, 96, PixelFormats.Pbgra32);
        
        DispatcherTimer timer = new DispatcherTimer();
        timer.Tick += DispatcherTimer_Tick;
        timer.Interval = TimeSpan.FromMilliseconds(1000.0 / TargetFps);
        timer.Start();
        
        OnInitializeScene();
    }
    
    protected virtual void OnInitializeScene() { }
    
    protected abstract void UpdateScene(double deltaTime);
    protected abstract void DrawScene(DrawingContext dc);
    
    private void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        // Calculate FPS (common)
        DateTime currentTime = DateTime.Now;
        double deltaTime = (currentTime - lastFrameTime).TotalSeconds;
        lastFrameTime = currentTime;
        
        if (deltaTime > 0)
        {
            double instantFps = 1.0 / deltaTime;
            fpsHistory.Enqueue(instantFps);
            if (fpsHistory.Count > fpsHistorySize)
                fpsHistory.Dequeue();
            currentFps = fpsHistory.Average();
        }
        
        // Update (child implements)
        UpdateScene(deltaTime);
        
        // Render (common pattern)
        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            DrawBackground(dc);
            DrawScene(dc);  // Child implements this
            DrawFpsCounter(dc);
        }
        
        renderBitmap.Clear();
        renderBitmap.Render(visual);
    }
    
    protected virtual void DrawBackground(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, 
            new Rect(0, 0, ScreenWidth, ScreenHeight));
    }
    
    private void DrawFpsCounter(DrawingContext dc)
    {
        CommonCustomDrawing.DrawFpsCounter(dc, currentFps, this, 
            ScreenWidth, ScreenHeight);
    }
}

// Usage
public partial class LoopWindowTest3 : BaseLoopWindow
{
    protected override int ScreenWidth => 256;
    protected override int ScreenHeight => 64;
    protected override int TargetFps => 60;
    
    private double rotationAngleX = 0;
    private double rotationAngleY = 0;
    private double rotationAngleZ = 0;
    
    protected override void UpdateScene(double deltaTime)
    {
        rotationAngleX += 30 * deltaTime;
        rotationAngleY += 21 * deltaTime;
        rotationAngleZ += 15 * deltaTime;
    }
    
    protected override void DrawScene(DrawingContext dc)
    {
        // Draw noise
        var noise = NoiseMethods.UniformRandomNoiseImage(ScreenWidth, ScreenHeight, 12);
        dc.DrawImage(noise, new Rect(0, 0, ScreenWidth, ScreenHeight));
        
        // Draw cube
        DrawCube(dc);
    }
    
    private void DrawCube(DrawingContext dc)
    {
        // ... cube logic ...
    }
}
```

#### مزایا:
- ✅ ساده‌تر از Interface
- ✅ کد مشترک فقط یکبار نوشته می‌شود
- ✅ ارث‌بری طبیعی و آشنا
- ✅ کد کمتر نسبت به Interface

#### معایب:
- ⚠️ C# فقط single inheritance دارد
- ⚠️ انعطاف‌پذیری کمتر از Interface
- ⚠️ تست کردن سخت‌تر از Interface
- ⚠️ Coupling بیشتر

---

### روش ۳: Composition with Strategy Pattern

**مفهوم:** استفاده از ترکیب اشیاء (Composition) به جای ارث‌بری

```csharp
// Rendering Engine (reusable)
public class RenderEngine
{
    public double CurrentFps { get; private set; }
    private DateTime lastFrameTime = DateTime.Now;
    private Queue<double> fpsHistory = new Queue<double>();
    private const int fpsHistorySize = 30;
    
    public double CalculateDeltaTime()
    {
        DateTime currentTime = DateTime.Now;
        double deltaTime = (currentTime - lastFrameTime).TotalSeconds;
        lastFrameTime = currentTime;
        return deltaTime;
    }
    
    public void UpdateFps(double deltaTime)
    {
        if (deltaTime > 0)
        {
            double instantFps = 1.0 / deltaTime;
            fpsHistory.Enqueue(instantFps);
            if (fpsHistory.Count > fpsHistorySize)
                fpsHistory.Dequeue();
            CurrentFps = fpsHistory.Average();
        }
    }
    
    public void RenderFrame(IDrawable drawable, RenderTargetBitmap target, Window window)
    {
        double deltaTime = CalculateDeltaTime();
        UpdateFps(deltaTime);
        
        drawable.Update(deltaTime);
        
        DrawingVisual visual = drawable.CreateVisual();
        target.Clear();
        target.Render(visual);
    }
}

// Drawable interface
public interface IDrawable
{
    void Update(double deltaTime);
    DrawingVisual CreateVisual();
    int Width { get; }
    int Height { get; }
}

// Scene configuration
public class SceneConfig
{
    public int Width { get; set; } = 480;
    public int Height { get; set; } = 360;
    public int TargetFps { get; set; } = 60;
    public Brush Background { get; set; } = Brushes.Transparent;
}

// Generic Render Window
public class GenericRenderWindow : Window
{
    private RenderEngine engine;
    private IDrawable drawable;
    private RenderTargetBitmap renderBitmap;
    private DispatcherTimer timer;
    
    public GenericRenderWindow(IDrawable drawable, SceneConfig config)
    {
        this.drawable = drawable;
        this.engine = new RenderEngine();
        
        InitializeComponent();
        
        renderBitmap = new RenderTargetBitmap(
            drawable.Width, drawable.Height, 96, 96, PixelFormats.Pbgra32);
        
        timer = new DispatcherTimer();
        timer.Tick += (s, e) => engine.RenderFrame(drawable, renderBitmap, this);
        timer.Interval = TimeSpan.FromMilliseconds(1000.0 / config.TargetFps);
        timer.Start();
    }
}

// Cube Scene
public class CubeScene : IDrawable
{
    public int Width { get; }
    public int Height { get; }
    
    private double rotationAngleX, rotationAngleY, rotationAngleZ;
    private const double rotationSpeed = 30;
    
    public CubeScene(int width, int height)
    {
        Width = width;
        Height = height;
    }
    
    public void Update(double deltaTime)
    {
        rotationAngleX += rotationSpeed * deltaTime;
        rotationAngleY += rotationSpeed * deltaTime * 0.7;
        rotationAngleZ += rotationSpeed * deltaTime * 0.5;
    }
    
    public DrawingVisual CreateVisual()
    {
        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            // Draw background
            var noise = NoiseMethods.UniformRandomNoiseImage(Width, Height, 12);
            dc.DrawImage(noise, new Rect(0, 0, Width, Height));
            
            // Draw cube
            DrawCube(dc);
        }
        return visual;
    }
    
    private void DrawCube(DrawingContext dc) { /* ... */ }
}
```

#### مزایا:
- ✅ ماژولار و قابل ترکیب
- ✅ `RenderEngine` قابل استفاده در جاهای دیگر (مثلا برای export video)
- ✅ تست و debug آسان‌تر
- ✅ انعطاف‌پذیری بالا

#### معایب:
- ⚠️ پیچیدگی بیشتر
- ⚠️ Layer اضافی

---

### روش ۴: MVVM Pattern

**مفهوم:** استفاده از معماری MVVM برای پروژه‌های بزرگ‌تر

```csharp
// ViewModel
public class RenderViewModel : INotifyPropertyChanged
{
    private double currentFps;
    public double CurrentFps
    {
        get => currentFps;
        set { currentFps = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<IDrawable> DrawableObjects { get; set; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    
    private DispatcherTimer timer;
    
    public RenderViewModel()
    {
        DrawableObjects = new ObservableCollection<IDrawable>();
        StartCommand = new RelayCommand(Start);
        StopCommand = new RelayCommand(Stop);
    }
    
    public void Update()
    {
        // Update logic
        foreach (var obj in DrawableObjects)
        {
            obj.Update(deltaTime);
        }
    }
    
    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

// View (XAML)
/*
<Window>
    <Grid>
        <Image Source="{Binding RenderTarget}" />
        <TextBlock Text="{Binding CurrentFps, StringFormat='FPS: {0:F1}'}" />
    </Grid>
</Window>
*/
```

#### مزایا:
- ✅ مناسب برای پروژه‌های بزرگ
- ✅ Data binding
- ✅ تست‌پذیری عالی
- ✅ جداسازی کامل UI از منطق

#### معایب:
- ⚠️ Over-engineering برای این پروژه
- ⚠️ نیاز به framework اضافی (مثل MVVM Light، Prism)
- ⚠️ منحنی یادگیری

---

## پیشنهاد نهایی

**برای این پروژه: ترکیب روش ۱ و ۳**

### ساختار پیشنهادی پوشه‌ها:

```
BasicBitmapManipulation/
├─ Rendering/
│   ├─ BaseRenderWindow.cs          (پنجره ژنریک)
│   ├─ BaseRenderWindow.xaml        (XAML ساده با یک Image)
│   ├─ ISceneRenderer.cs            (Interface برای scene ها)
│   ├─ RenderConfig.cs              (تنظیمات رندر)
│   └─ RenderEngine.cs              (موتور رندر - اختیاری)
├─ Scenes/
│   ├─ CubeSceneRenderer.cs         (مکعب چرخان)
│   ├─ RotatingRectSceneRenderer.cs (مستطیل‌های چرخان)
│   └─ RandomShapesSceneRenderer.cs (اشکال تصادفی)
├─ DrawCommon/
│   └─ CommonCustomDrawing.cs       (موجود)
└─ MainWindow.xaml.cs
    // در MainWindow:
    var cubeScene = new CubeSceneRenderer(256, 64);
    var window = new BaseRenderWindow(cubeScene);
    window.Show();
```

### مراحل پیاده‌سازی:

1. ✅ **مرحله ۱:** ساخت `ISceneRenderer` interface
2. ✅ **مرحله ۲:** ساخت `BaseRenderWindow` با تمام منطق مشترک
3. ✅ **مرحله ۳:** استخراج منطق `LoopWindowTest3` به `CubeSceneRenderer`
4. ✅ **مرحله ۴:** استخراج منطق `LoopWindowTest2` به `RotatingRectSceneRenderer`
5. ✅ **مرحله ۵:** استخراج منطق `LoopWindowTest1` به `RandomShapesSceneRenderer`
6. ✅ **مرحله ۶:** تست و بررسی عملکرد
7. ✅ **مرحله ۷:** حذف پنجره‌های قدیمی (اختیاری)

---

## مثال کد کامل (خلاصه)

```csharp
// File: Rendering/ISceneRenderer.cs
public interface ISceneRenderer
{
    int ScreenWidth { get; }
    int ScreenHeight { get; }
    void Initialize();
    void Update(double deltaTime);
    void Draw(DrawingContext dc);
}

// File: Rendering/BaseRenderWindow.cs
public class BaseRenderWindow : Window
{
    private readonly ISceneRenderer scene;
    private RenderTargetBitmap renderBitmap;
    private Image imageControl;
    // ... FPS tracking fields ...
    
    public BaseRenderWindow(ISceneRenderer sceneRenderer)
    {
        scene = sceneRenderer;
        InitializeWindow();
        scene.Initialize();
        StartRenderLoop();
    }
    
    private void InitializeWindow()
    {
        Title = "Render Window";
        Width = 800;
        Height = 600;
        
        imageControl = new Image
        {
            RenderOptions.BitmapScalingMode = BitmapScalingMode.NearestNeighbor
        };
        
        Grid grid = new Grid { Background = Brushes.LightBlue };
        grid.Children.Add(imageControl);
        Content = grid;
        
        renderBitmap = new RenderTargetBitmap(
            scene.ScreenWidth, scene.ScreenHeight, 96, 96, PixelFormats.Pbgra32);
        imageControl.Source = renderBitmap;
    }
    
    private void StartRenderLoop()
    {
        DispatcherTimer timer = new DispatcherTimer();
        timer.Tick += RenderFrame;
        timer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60);
        timer.Start();
    }
    
    private void RenderFrame(object sender, EventArgs e)
    {
        double deltaTime = CalculateDeltaTime();
        UpdateFps(deltaTime);
        
        scene.Update(deltaTime);
        
        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            scene.Draw(dc);
            CommonCustomDrawing.DrawFpsCounter(dc, currentFps, this, 
                scene.ScreenWidth, scene.ScreenHeight);
        }
        
        renderBitmap.Clear();
        renderBitmap.Render(visual);
    }
}

// File: Scenes/CubeSceneRenderer.cs
public class CubeSceneRenderer : ISceneRenderer
{
    public int ScreenWidth { get; }
    public int ScreenHeight { get; }
    
    private double rotationX, rotationY, rotationZ;
    
    public CubeSceneRenderer(int width = 480, int height = 360)
    {
        ScreenWidth = width;
        ScreenHeight = height;
    }
    
    public void Initialize() { }
    
    public void Update(double deltaTime)
    {
        rotationX += 30 * deltaTime;
        rotationY += 21 * deltaTime;
        rotationZ += 15 * deltaTime;
    }
    
    public void Draw(DrawingContext dc)
    {
        // Background
        var noise = NoiseMethods.UniformRandomNoiseImage(ScreenWidth, ScreenHeight, 12);
        dc.DrawImage(noise, new Rect(0, 0, ScreenWidth, ScreenHeight));
        
        // Cube
        DrawCube(dc);
    }
    
    private void DrawCube(DrawingContext dc) { /* ... */ }
}

// Usage in MainWindow:
private void BaseButton_Click(object sender, RoutedEventArgs e)
{
    if (e.OriginalSource is Button button && button.Tag is string windowType)
    {
        ISceneRenderer scene = windowType switch
        {
            "LoopWindowTest1" => new RandomShapesSceneRenderer(512, 512),
            "LoopWindowTest2" => new RotatingRectSceneRenderer(480, 360),
            "LoopWindowTest3" => new CubeSceneRenderer(256, 64),
            _ => null
        };
        
        if (scene != null)
        {
            BaseRenderWindow window = new BaseRenderWindow(scene);
            window.Owner = this;
            window.Show();
        }
    }
}
```

---

## نتیجه‌گیری

این refactoring باعث می‌شود:

1. ✅ **کد تکراری حذف شود** - تمام منطق FPS، Timer، Render یکبار نوشته می‌شود
2. ✅ **تست‌پذیری افزایش یابد** - می‌توان هر Scene را جداگانه تست کرد
3. ✅ **خوانایی بهبود یابد** - هر Scene فقط منطق خودش را دارد
4. ✅ **توسعه‌پذیری بالا رود** - اضافه کردن Scene جدید بسیار آسان
5. ✅ **Single Responsibility** - هر کلاس یک مسئولیت دارد

---

**نکته:** این مستند فقط پیشنهاد است. قبل از شروع refactoring، مطمئن شوید که:
- کد فعلی را commit کرده‌اید
- تست‌های لازم را انجام داده‌اید
- زمان کافی برای refactoring دارید

---

**تهیه‌کننده:** AI Assistant  
**پروژه:** BitmapResearcher  
**فایل:** Refactoring-Proposals.md

