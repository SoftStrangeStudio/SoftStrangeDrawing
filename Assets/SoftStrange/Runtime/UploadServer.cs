using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SoftStrange {
 public sealed class UploadServer : IDisposable {
  public const int MaxBytes=12*1024*1024;
  public readonly ConcurrentQueue<byte[]> Pending=new ConcurrentQueue<byte[]>();
  public string Error;
  public volatile string Snapshot="{}",Events="{\"events\":[]}",Commands="{\"commands\":[]}";
  public sealed class CommandRequest {
   public string Json,Result;
   public readonly ManualResetEventSlim Completed=new ManualResetEventSlim(false);
  }
  public readonly ConcurrentQueue<CommandRequest> CommandQueue=new ConcurrentQueue<CommandRequest>();
  readonly string html;
  readonly TcpListener listener=new TcpListener(IPAddress.Loopback,8787);
  volatile bool running;
  int pendingCount;
  public UploadServer(string html){this.html=html;}
  public void Start(){try{listener.Start();running=true;new Thread(Loop){IsBackground=true,Name="Image upload"}.Start();}catch(Exception e){Error=e.Message;}}
  void Loop(){while(running){try{using(var client=listener.AcceptTcpClient()){client.ReceiveTimeout=5000;client.SendTimeout=5000;Handle(client.GetStream());}}catch(Exception e){if(running)Error=e.Message;}}}
  void Handle(NetworkStream stream){
   using(var header=new MemoryStream()){
    int state=0;
    while(header.Length<8192&&state<4){int b=stream.ReadByte();if(b<0)return;header.WriteByte((byte)b);state=(state==0&&b==13)?1:(state==1&&b==10)?2:(state==2&&b==13)?3:(state==3&&b==10)?4:0;}
    if(state!=4){Reply(stream,400,"Header too large");return;}
    string[] lines=Encoding.ASCII.GetString(header.ToArray()).Split(new[]{"\r\n"},StringSplitOptions.None);
    if(lines[0].StartsWith("GET /api/state ")){Reply(stream,200,Snapshot,"application/json");return;}
    if(lines[0].StartsWith("GET /api/events ")){Reply(stream,200,Events,"application/json");return;}
    if(lines[0].StartsWith("GET /api/commands ")){Reply(stream,200,Commands,"application/json");return;}
    bool command=lines[0].StartsWith("POST /api/command ");
    if(lines[0]=="GET / HTTP/1.1"||lines[0]=="GET / HTTP/1.0"){Reply(stream,200,html,"text/html; charset=utf-8");return;}
    if(!command&&lines[0]!="POST /upload HTTP/1.1"){Reply(stream,404,"Not found");return;}
    int length=0;
    foreach(string line in lines){if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))int.TryParse(line.Substring(15).Trim(),out length);}
    if(length<(command?2:16)||length>(command?65536:MaxBytes)){Reply(stream,413,"Choose a PNG/JPEG under 12 MB");return;}
    if(pendingCount>=2){Reply(stream,429,"Please wait for the previous image");return;}
    byte[] bytes=new byte[length];int read=0;
    while(read<length){int count=stream.Read(bytes,read,length-read);if(count==0){Reply(stream,400,"Incomplete image");return;}read+=count;}
    if(command){
     if(CommandQueue.Count>=16){Reply(stream,429,"Command queue is full");return;}
     var request=new CommandRequest{Json=Encoding.UTF8.GetString(bytes)};CommandQueue.Enqueue(request);
     if(request.Completed.Wait(2000)){Reply(stream,200,request.Result,"application/json");request.Completed.Dispose();}
     else Reply(stream,202,"{\"accepted\":true,\"pending\":true}","application/json");return;
    }
    if(!ImageSizeAllowed(bytes)){Reply(stream,415,"Use a PNG/JPEG with dimensions up to 4096 pixels");return;}
    Interlocked.Increment(ref pendingCount);Pending.Enqueue(bytes);Reply(stream,202,"Transferred. Check the headset for image confirmation.");
   }
  }
  public bool Take(out byte[] bytes){if(Pending.TryDequeue(out bytes)){Interlocked.Decrement(ref pendingCount);return true;}return false;}
  // Check dimensions before Unity allocates decoded pixels.
  public static bool ImageSizeAllowed(byte[] b){
   if(b.Length>=24&&b[0]==137&&b[1]==80&&b[2]==78&&b[3]==71){long w=((long)b[16]<<24)|(b[17]<<16)|(b[18]<<8)|b[19],h=((long)b[20]<<24)|(b[21]<<16)|(b[22]<<8)|b[23];return w>0&&h>0&&w<=4096&&h<=4096;}
   if(b.Length<4||b[0]!=255||b[1]!=216)return false;
   int p=2;
   while(p+3<b.Length){if(b[p++]!=255)return false;while(p<b.Length&&b[p]==255)p++;if(p>=b.Length)return false;int marker=b[p++];if(marker==217||marker==218)return false;if(marker==1||(marker>=208&&marker<=215))continue;if(p+1>=b.Length)return false;int n=(b[p]<<8)|b[p+1];if(n<2||p+n>b.Length)return false;
    if((marker>=192&&marker<=195)||(marker>=197&&marker<=199)||(marker>=201&&marker<=203)||(marker>=205&&marker<=207)){if(n<8)return false;int h=(b[p+3]<<8)|b[p+4],w=(b[p+5]<<8)|b[p+6];return w>0&&h>0&&w<=4096&&h<=4096;}p+=n;
   }return false;
  }
  static void Reply(NetworkStream stream,int code,string body,string type="text/plain; charset=utf-8"){
   byte[] bytes=Encoding.UTF8.GetBytes(body);byte[] head=Encoding.ASCII.GetBytes($"HTTP/1.1 {code} Response\r\nContent-Type: {type}\r\nContent-Length: {bytes.Length}\r\nConnection: close\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\n\r\n");stream.Write(head,0,head.Length);stream.Write(bytes,0,bytes.Length);
  }
  public void Dispose(){running=false;listener.Stop();}
 }
}
