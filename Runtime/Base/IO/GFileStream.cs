using System.IO;

namespace IG.IO{
    public class GFileStream : FileStream{
        public GFileStream(string path, FileMode mode) : base(path, mode){
            this.SetLength(0);
            this.Seek(0, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing){
            this.Flush();
            base.Dispose(disposing);
        }
    }
}