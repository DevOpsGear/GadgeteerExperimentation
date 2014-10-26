using System.Threading;
using Microsoft.SPOT;
using System;
using System.IO;

namespace GadgeteerExperimentation
{
    public partial class Program
    {
        private void ProgramStarted()
        {
            var th = new Thread(Test);
            th.Start();
        }

        private void Test()
        {
            Debug.Print("Program started - memory = " + Debug.GC(false));

            var isInserted = Mainboard.IsSDCardInserted;
            var isMounted = Mainboard.IsSDCardMounted;
            if(!isMounted)
                Mainboard.MountStorageDevice(@"SD");

            var mb = Mainboard;
            var sd = mb.SDCardStorageDevice;
            var vi = sd.Volume;
            //vi.Format("FAT", 0, "TEST", true);
            vi.Refresh();
            var totalSize = vi.TotalSize;
            var freeSpace = vi.TotalFreeSpace;

            Debug.Print("totalSize = " + totalSize);
            Debug.Print("freeSpace = " + freeSpace);

            for (var preAlloc = 20000; preAlloc < 40000; preAlloc += 1000)
            {
                Thread.Sleep(250);
                Debug.GC(true);
                RunTests(preAlloc);
            }
        }

        private static void RunTests(int preAlloc)
        {
            Debug.Print("STARTING RUN (PREALLOCATING " + preAlloc + " BYTES)");
            Debug.Print("Prior to alloc, memory = " + Debug.GC(false));

            var preAllocatedMemory = new byte[preAlloc];
            Debug.Print("Beginning - memory = " + Debug.GC(false));

            const string directory = @"\SD\experimentation";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            const int length = 500;
            const int countFiles = 500;
            var sizes = new int[countFiles];
            var rnd = new Random();
            for (var idx = 0; idx < countFiles; idx++)
            {
                Thread.Sleep(20);

                sizes[idx] = length;
                var data = new byte[length];
                rnd.NextBytes(data);

                var fileName = "file-" + idx + ".txt";
                var path = Path.Combine(directory, fileName);
                File.WriteAllBytes(path, data);
                if (idx%25 == 0)
                    Debug.Print("Writing file " + idx + "; memory = " + Debug.GC(false));
            }

            for (var idx = 0; idx < countFiles; idx++)
            {
                Thread.Sleep(20);

                var fileName = "file-" + idx + ".txt";
                var path = Path.Combine(directory, fileName);
                var data = File.ReadAllBytes(path);
                if (idx%25 == 0)
                    Debug.Print("Reading file " + idx + "; memory = " + Debug.GC(false));
                if (data.Length != sizes[idx])
                    Debug.Print("*** LENGTH MISMATCH FOR FILE " + idx + "!!!");
            }
            Debug.Print("Ending - memory = " + Debug.GC(false));
            Debug.Print("ENDING RUN (PREALLOCATING " + preAlloc + " BYTES)");
            Debug.Print("");
        }
    }
}