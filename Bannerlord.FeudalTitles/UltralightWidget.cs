using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Bannerlord.UIExtenderEx.Attributes;
using ImpromptuNinjas.UltralightSharp;
using ImpromptuNinjas.UltralightSharp.Enums;
using Serilog;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.TwoDimension;
using Path = System.IO.Path;
using Renderer = ImpromptuNinjas.UltralightSharp.Renderer;
using String = ImpromptuNinjas.UltralightSharp.String;
using Texture = TaleWorlds.Engine.Texture;
using View = ImpromptuNinjas.UltralightSharp.View;

namespace Bannerlord.FeudalTitles
{
    public class UltralightVM : ViewModel
    {
        [DataSourceMethod]
        private void HelloWorld()
        {
            //Empty
        }
    }
    
    public class UltralightScreen : ScreenBase
    {
        private GauntletLayer _gauntletLayer;
        private UltralightVM _dataSource;
        private IGauntletMovie _movie;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataSource = new UltralightVM();
            _gauntletLayer = new GauntletLayer(1);
            AddLayer(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.IsFocusLayer = true;
            _movie = _gauntletLayer.LoadMovie("UltralightMovie", _dataSource);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ScreenManager.TrySetFocus(this._gauntletLayer);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            this._gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            RemoveLayer(_gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;
            _movie = null;
        }
    }
    
    public unsafe class UltralightWidget : Widget
    {
        private Renderer* _renderer;
        private View* _view;

        private string _url;
        private bool _isDomReady;
        private bool _isWindow;
        private bool _failed;
        private bool _isLoaded;
        private bool _willRender;
        private bool _isWindowReady;


        public string Url
        {
            get => _url;
            set
            {
                if (_view == null) return;

                _url = value;
                var s = String.Create(value);
                _view->LoadUrl(s);
                s->Destroy();
            }
        }
        
        
        public UltralightWidget(UIContext context) : base(context)
        {
            Initialize();
        }

        ~UltralightWidget()
        {
            _view->Destroy();
            _view = null;
            _renderer->Destroy();
            _renderer = null;
            
            _isDomReady = false;
            _failed = false;
            _isLoaded = false;
            _isWindowReady = false;
            _url = "";
        }

        
        public async void Initialize()
        {
            //Sprite = new SpriteGeneric("new", new SpritePart("newPart", new SpriteCategory("newCat", UIResourceManager.SpriteData, 0), 640, 480));
            var asmPath = new Uri(typeof(UltralightWidget).Assembly.CodeBase!).LocalPath;
            var asmDir = Path.GetDirectoryName(asmPath)!;
            var tempDir = Path.GetTempPath();

            string storagePath;
            do
            {
                storagePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
            } while (Directory.Exists(storagePath) || File.Exists(storagePath));

            var cfg = Config.Create();

            {
                var cachePath = String.Create(Path.Combine(storagePath, "Cache"));
                cfg->SetCachePath(cachePath);
                cachePath->Destroy();
            }

            {
                var resourcePath = String.Create(Path.Combine(asmDir, "resources"));
                cfg->SetResourcePath(resourcePath);
                resourcePath->Destroy();
            }

            cfg->SetUseGpuRenderer(false);
            cfg->SetEnableImages(true);
            cfg->SetEnableJavaScript(false);

            AppCore.EnablePlatformFontLoader();

            {
                var assetsPath = String.Create(Path.Combine(asmDir, "assets"));
                AppCore.EnablePlatformFileSystem(assetsPath);
                assetsPath->Destroy();
            }

            _renderer = Renderer.Create(cfg);
            var sessionName = String.Create("Demo");
            var session = Session.Create(_renderer, false, sessionName);

            _view = View.Create(_renderer, 640, 480, false, session);

            var gch = GCHandle.Alloc(this);
            var gchPtr = (void*) GCHandle.ToIntPtr(gch);

            _view->SetAddConsoleMessageCallback((userData, caller, source, level, msg, lineNumber, columnNumber, sourceId) =>
            {
                switch (level)
                {
                    default:
                        Log.Debug($"[Ultralight Console] [{level}] {sourceId->Read()}:{lineNumber}:{columnNumber} {msg->Read()}");
                        break;
                    case MessageLevel.Error:
                        Log.Error($"[Ultralight Console] {sourceId->Read()}:{lineNumber}:{columnNumber} {msg->Read()}");
                        break;
                    case MessageLevel.Warning:
                        Log.Warning($"[Ultralight Console] {sourceId->Read()}:{lineNumber}:{columnNumber} {msg->Read()}");
                        break;
                }
            }, gchPtr);

            _view->SetDomReadyCallback((userData, caller, id, isMainFrame, readyUrl) =>
            {
                if (!isMainFrame)
                    return;

                var r = (UltralightWidget) GCHandle.FromIntPtr((IntPtr) userData).Target;

                r._isDomReady = true;
            }, gchPtr);

            _view->SetFailLoadingCallback((userData, caller, id, isMainFrame, failedUrl, failDesc, errorDomain, errorCode) =>
            {
                if (!isMainFrame)
                    return;

                var r = (UltralightWidget) GCHandle.FromIntPtr((IntPtr) userData).Target;

                r._failed = true;
                r._isLoaded = true;
            }, gchPtr);

            _view->SetFinishLoadingCallback((userData, caller, id, isMainFrame, finishedUrl) =>
            {
                if (!isMainFrame)
                    return;

                var r = (UltralightWidget) GCHandle.FromIntPtr((IntPtr) userData).Target;

                r._failed = false;
                r._isLoaded = true;
            }, gchPtr);

            _view->SetWindowObjectReadyCallback((userData, caller, id, isMainFrame, readyUrl) =>
            {
                if (!isMainFrame)
                    return;

                var r = (UltralightWidget) GCHandle.FromIntPtr((IntPtr) userData).Target;

                r._isWindowReady = true;
            }, gchPtr);

            _view->SetChangeUrlCallback((userData, caller, newUrl) =>
            {
                if (!caller->IsLoading())
                    return;

                var r = (UltralightWidget) GCHandle.FromIntPtr((IntPtr) userData).Target;

                r._isDomReady = false;
                r._failed = false;
                r._isLoaded = false;
                r._isWindowReady = false;
            }, gchPtr);

            Url = _url;
        }

        public void LoadHtml(string htmlString) {
            if (_view == null)
                return;

            var s = String.Create(htmlString);
            _view->LoadHtml(s);
            s->Destroy();
        }

        protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
        {
            _willRender = true;
            if (_renderer == null || _view == null)
                return;
            
            Ultralight.Update(_renderer);
            Ultralight.Render(_renderer);
            
            var surface = _view->GetSurface();
            var bitmap = surface->GetBitmap();
            var pixels = bitmap->LockPixels();
            var buffer = Unsafe.Read<byte[]>(pixels);
            Texture texture = Texture.CreateFromMemory(buffer);
            drawContext.DrawSprite(Sprite, new SimpleMaterial(new TaleWorlds.TwoDimension.Texture(new EngineTexture(texture))), 1,1,1,640,480,false,false);
            //drawContext.Draw( 0,0,new SimpleMaterial(new TaleWorlds.TwoDimension.Texture(new EngineTexture(texture))),DrawObject2D.CreateQuad(Vector2.One),640,480);

            bitmap->UnlockPixels();
            _willRender = false;
        }
    }
}