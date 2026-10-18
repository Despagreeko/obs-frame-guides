using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FrameGuides {
 static class Program {
  [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
  [STAThread] static void Main() {
   SetProcessDPIAware(); Application.EnableVisualStyles();
   Application.SetCompatibleTextRenderingDefault(false);
   Application.Run(new Guide());
  }
 }
 public class Guide : Form {
  [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int n);
  [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int n, int v);
  [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr h, int id, uint mod, uint key);
  [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr h, int id);
  [DllImport("user32.dll")] static extern bool SetWindowDisplayAffinity(IntPtr h, uint affinity);
  public int L, R, T, B, Thickness = 2;
  public Color Ink = Color.Lime;
  public bool Locked, GuidesVisible = true, IncludeInCapture, CaptureSettingApplied;
  int drag = -1;
  PanelWindow panel;
  NotifyIcon tray;
  string settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OBSFrameGuides", "settings.txt");
  public bool HotkeysAvailable = true, CaptureExcluded;
  public Guide() {
   Text = "OBS Frame Guides Overlay"; FormBorderStyle = FormBorderStyle.None;
   ShowInTaskbar = false; StartPosition = FormStartPosition.Manual;
   Bounds = SystemInformation.VirtualScreen; BackColor = Color.Magenta;
   TransparencyKey = Color.Magenta; TopMost = true; DoubleBuffered = true;
   ResetBounds(); LoadSettings();
   MouseDown += delegate(object s, MouseEventArgs e) { if (!Locked && e.Button == MouseButtons.Left) { drag = Nearest(e.Location); Capture = drag >= 0; } };
   MouseMove += delegate(object s, MouseEventArgs e) {
    if (drag >= 0) {
     if (drag == 0) L = Math.Max(0, Math.Min(R-1,e.X));
     if (drag == 1) R = Math.Max(L+1, Math.Min(ClientSize.Width-1,e.X));
     if (drag == 2) T = Math.Max(0, Math.Min(B-1,e.Y));
     if (drag == 3) B = Math.Max(T+1, Math.Min(ClientSize.Height-1,e.Y));
     Changed();
    } else { int n = Nearest(e.Location); Cursor = n < 0 ? Cursors.Default : n < 2 ? Cursors.SizeWE : Cursors.SizeNS; }
   };
   MouseUp += delegate { drag = -1; Capture = false; Save(); };
   tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "OBS Frame Guides", Visible = true };
   var menu = new ContextMenuStrip();
   menu.Items.Add("Open controls", null, delegate { OpenControls(); });
   menu.Items.Add("Lock / unlock guides", null, delegate { SetLocked(!Locked); });
   menu.Items.Add("Show / hide guides", null, delegate { ToggleVisible(); });
   menu.Items.Add("Exit", null, delegate { Close(); }); tray.ContextMenuStrip = menu;
   tray.DoubleClick += delegate { OpenControls(); };
  }
  protected override bool ShowWithoutActivation { get { return true; } }
  protected override CreateParams CreateParams { get { var p = base.CreateParams; p.ExStyle |= 0x08000000 | 0x80; return p; } }
  protected override void OnShown(EventArgs e) {
   base.OnShown(e);
   HotkeysAvailable = RegisterHotKey(Handle, 1, 0x4003, (uint)Keys.L);
   HotkeysAvailable = RegisterHotKey(Handle, 2, 0x4003, (uint)Keys.G) && HotkeysAvailable;
   SetIncludeInCapture(IncludeInCapture);
   SetLocked(Locked); OpenControls();
  }
  void OpenControls() {
   if (panel == null || panel.IsDisposed) panel = new PanelWindow(this);
   panel.Show(); panel.WindowState = FormWindowState.Normal; panel.Activate(); panel.RefreshValues();
  }
  public void ResetBounds() {
   Rectangle p = Screen.PrimaryScreen.Bounds;
   L = p.Left - Left + p.Width / 10; R = p.Left - Left + p.Width * 9 / 10;
   T = p.Top - Top + p.Height / 10; B = p.Top - Top + p.Height * 9 / 10;
   Changed();
  }
  public void FitScreen(Screen screen) {
   Rectangle r = screen.Bounds;
   L = r.Left-Left; R = r.Right-Left-1; T = r.Top-Top; B = r.Bottom-Top-1; Changed(); Save();
  }
  public void Changed() { Invalidate(); if (panel != null && !panel.IsDisposed) panel.RefreshValues(); }
  public void SetLocked(bool value) {
   Locked = value;
   if (IsHandleCreated) { int style = GetWindowLong(Handle, -20); SetWindowLong(Handle, -20, value ? style | 0x20 : style & ~0x20); }
   Changed(); Save();
  }
  public void ToggleVisible() { GuidesVisible = !GuidesVisible; Changed(); }
  public void SetIncludeInCapture(bool value) {
   if (IsHandleCreated) {
    CaptureSettingApplied = SetWindowDisplayAffinity(Handle, value ? 0u : 0x11u);
    if (CaptureSettingApplied) { IncludeInCapture = value; CaptureExcluded = !value; }
   } else IncludeInCapture = value;
   Changed(); Save();
  }
  int Nearest(Point p) {
   if (!GuidesVisible) return -1;
   int[] d = { Math.Abs(p.X-L), Math.Abs(p.X-R), Math.Abs(p.Y-T), Math.Abs(p.Y-B) };
   int best = 0; for (int i=1;i<4;i++) if(d[i]<d[best]) best=i;
   return d[best] <= Math.Max(8, Thickness+3) ? best : -1;
  }
  protected override void WndProc(ref Message m) {
   if (m.Msg == 0x312) { if(m.WParam.ToInt32()==1) SetLocked(!Locked); else if(m.WParam.ToInt32()==2) ToggleVisible(); return; }
   if (m.Msg == 0x84) {
    long v = m.LParam.ToInt64(); Point p = PointToClient(new Point((short)(v & 65535), (short)((v >> 16) & 65535)));
    m.Result = new IntPtr(Locked || Nearest(p)<0 ? -1 : 1); return;
   }
   if (m.Msg == 0x7E) { Bounds = SystemInformation.VirtualScreen; Clamp(); Changed(); }
   base.WndProc(ref m);
  }
  protected override void OnPaint(PaintEventArgs e) {
   base.OnPaint(e); if (!GuidesVisible) return;
   using(var pen = new Pen(Ink,Thickness)) {
    e.Graphics.DrawLine(pen,L,0,L,Height); e.Graphics.DrawLine(pen,R,0,R,Height);
    e.Graphics.DrawLine(pen,0,T,Width,T); e.Graphics.DrawLine(pen,0,B,Width,B);
   }
   if (!Locked) {
    using(var brush = new SolidBrush(Ink)) {
     int x=(L+R)/2, y=(T+B)/2;
     e.Graphics.FillRectangle(brush,L-5,y-12,11,25); e.Graphics.FillRectangle(brush,R-5,y-12,11,25);
     e.Graphics.FillRectangle(brush,x-12,T-5,25,11); e.Graphics.FillRectangle(brush,x-12,B-5,25,11);
    }
   }
  }
  void Clamp() { L=Math.Max(0,Math.Min(Width-2,L)); R=Math.Max(L+1,Math.Min(Width-1,R)); T=Math.Max(0,Math.Min(Height-2,T)); B=Math.Max(T+1,Math.Min(Height-1,B)); Thickness=Math.Max(1,Math.Min(10,Thickness)); }
  void LoadSettings() {
   try { var a=File.ReadAllLines(settings); if(a.Length>=9) {
    L=int.Parse(a[0]); R=int.Parse(a[1]); T=int.Parse(a[2]); B=int.Parse(a[3]); Thickness=int.Parse(a[4]); Ink=Color.FromArgb(int.Parse(a[5])); Locked=bool.Parse(a[6]);
    if(a.Length>=10) IncludeInCapture=bool.Parse(a[9]);
    if(int.Parse(a[7])!=Width || int.Parse(a[8])!=Height) ResetBounds(); Clamp();
   }} catch { } 
  }
  public void Save() {
   try { Directory.CreateDirectory(Path.GetDirectoryName(settings)); File.WriteAllLines(settings,new string[] { L.ToString(),R.ToString(),T.ToString(),B.ToString(),Thickness.ToString(),Ink.ToArgb().ToString(),Locked.ToString(),Width.ToString(),Height.ToString(),IncludeInCapture.ToString() }); } catch { }
  }
  protected override void OnFormClosed(FormClosedEventArgs e) { Save(); UnregisterHotKey(Handle,1); UnregisterHotKey(Handle,2); tray.Dispose(); if(panel!=null) panel.Dispose(); base.OnFormClosed(e); }
 }
 public class PanelWindow : Form {
  Guide g; NumericUpDown[] coords = new NumericUpDown[4]; CheckBox locked, visible, capture;
  Label captureStatus;
  bool updating;
  public PanelWindow(Guide guide) {
   g=guide; Text="OBS Frame Guides"; ClientSize=new Size(370,482); FormBorderStyle=FormBorderStyle.FixedDialog;
   MaximizeBox=false; TopMost=true; StartPosition=FormStartPosition.CenterScreen;
   Font=new Font("Segoe UI",9F); BackColor=Color.FromArgb(245,247,250);
   AddLabel("Frame your shot",18,14,330,28,14);
   AddLabel("Drag the lines, or enter their pixel positions below.",18,48,340,25,9);
   string[] names={"Left","Right","Top","Bottom"};
   for(int i=0;i<4;i++) {
    int index=i, x=18+(i%2)*174, y=85+(i/2)*56;
    AddLabel(names[i],x,y,70,22,9);
    var n=new NumericUpDown { Location=new Point(x+66,y-1), Size=new Size(92,25), Minimum=0, Maximum=100000 };
    coords[i]=n; Controls.Add(n);
    n.ValueChanged+=delegate { if(updating)return; int value=(int)n.Value;
     if(index==0)g.L=Math.Min(value,g.R-1); if(index==1)g.R=Math.Max(g.L+1,Math.Min(g.Width-1,value));
     if(index==2)g.T=Math.Min(value,g.B-1); if(index==3)g.B=Math.Max(g.T+1,Math.Min(g.Height-1,value));
     g.Changed(); g.Save();
    };
   }
   AddLabel("Line width",18,199,78,25,9);
   var width=new NumericUpDown { Location=new Point(100,197), Size=new Size(58,25), Minimum=1,Maximum=10,Value=g.Thickness }; Controls.Add(width);
   width.ValueChanged+=delegate { g.Thickness=(int)width.Value; g.Changed();g.Save(); };
   Button color=Button("Line color",190,194,160,30); color.Click+=delegate { using(var d=new ColorDialog { Color=g.Ink, FullOpen=true }) { if(d.ShowDialog()==DialogResult.OK) { g.Ink=d.Color.ToArgb()==Color.Magenta.ToArgb()?Color.FromArgb(254,0,255):d.Color; g.Changed();g.Save(); } } };
   locked=new CheckBox { Text="Lock guides (click through)",Location=new Point(18,240),AutoSize=true }; Controls.Add(locked);
   locked.CheckedChanged+=delegate { if(!updating)g.SetLocked(locked.Checked); };
   visible=new CheckBox { Text="Show guides",Location=new Point(18,269),AutoSize=true }; Controls.Add(visible);
   visible.CheckedChanged+=delegate { if(!updating)g.ToggleVisible(); };
   capture=new CheckBox { Text="Include guides in screen capture",Location=new Point(18,298),AutoSize=true }; Controls.Add(capture);
   capture.CheckedChanged+=delegate { if(!updating)g.SetIncludeInCapture(capture.Checked); };
   Button reset=Button("Reset inset",18,338,106,30); reset.Click+=delegate {g.ResetBounds();g.Save();};
   Button fit=Button("Fit monitor",132,338,106,30); fit.Click+=delegate {
    var m=new ContextMenuStrip(); foreach(Screen s in Screen.AllScreens) { Screen target=s; m.Items.Add(s.DeviceName+" ("+s.Bounds.Width+" x "+s.Bounds.Height+")",null,delegate {g.FitScreen(target);}); } m.Show(fit,new Point(0,fit.Height));
   };
   Button hide=Button("To tray",246,338,104,30); hide.Click+=delegate {Hide();};
   AddLabel("Ctrl + Alt + L  lock / unlock\nCtrl + Alt + G  show / hide",18,381,330,44,9);
   captureStatus=new Label { Location=new Point(18,429),Size=new Size(330,44),Font=new Font("Segoe UI",8),ForeColor=Color.FromArgb(35,43,55) }; Controls.Add(captureStatus);
   FormClosing+=delegate(object s,FormClosingEventArgs e) { if(e.CloseReason==CloseReason.UserClosing) { e.Cancel=true;Hide(); } };
   RefreshValues();
  }
  void AddLabel(string text,int x,int y,int w,int h,float size) { Controls.Add(new Label {Text=text,Location=new Point(x,y),Size=new Size(w,h),Font=new Font("Segoe UI",size),ForeColor=Color.FromArgb(35,43,55)}); }
  Button Button(string text,int x,int y,int w,int h) {var b=new Button {Text=text,Location=new Point(x,y),Size=new Size(w,h)};Controls.Add(b);return b;}
  public void RefreshValues() {
   updating=true; int[] v={g.L,g.R,g.T,g.B}; for(int i=0;i<4;i++)coords[i].Value=Math.Max(0,Math.Min(100000,v[i]));
   if(locked!=null)locked.Checked=g.Locked;if(visible!=null)visible.Checked=g.GuidesVisible;
   if(capture!=null)capture.Checked=g.IncludeInCapture;
   if(captureStatus!=null)captureStatus.Text=(!g.CaptureSettingApplied ? "Windows could not apply the capture setting." : g.IncludeInCapture ? "Guides included in screen capture. Check OBS preview." : "Guides excluded from supported screen captures.") + (g.HotkeysAvailable ? "" : "\nShortcut in use: use the controls or tray menu.");
   updating=false;
  }
 }
}
