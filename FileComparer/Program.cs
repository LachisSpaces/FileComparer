using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace FileComparer
{
   class Program
   {
      static string _strApplicationPath = null;

      static void Main(string[] args)
      {
         _strApplicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
         string strFinalResult = "Files processed succesfully";

         Console.WriteLine("File comparer");
         Console.WriteLine("Please enter folder which should be cleaned:");
         string strPathCleaning = Console.ReadLine();
         Console.WriteLine("Please enter folder for comparison:");
         string strPathCompare = Console.ReadLine();
         Console.WriteLine("");

         try
         {

            DirectoryInfo dir1 = new DirectoryInfo(strPathCleaning);
            DirectoryInfo dir2 = new DirectoryInfo(strPathCompare);

            // Take a snapshot of the file system.
            IEnumerable<FileInfo> list1 = dir1.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<FileInfo> list2 = dir2.GetFiles("*.*", SearchOption.AllDirectories);

            //A custom file comparer defined below
            FileCompare myFileCompare = new FileCompare();

            // This query determines whether the two folders contain identical file lists, based on the custom file comparer that is defined in the FileCompare class. 
            // The query executes immediately because it returns a bool. 
            if (list1.SequenceEqual(list2, myFileCompare))
               Console.WriteLine("the two folders are the same");
            else
               Console.WriteLine("The two folders are not the same");
            Console.WriteLine("");

            // Find the common files. It produces a sequence and doesn't execute until the foreach statement. 
            var queryCommonFiles = list1.Intersect(list2, myFileCompare);
            if (queryCommonFiles.Count() > 0)
            {
               Console.WriteLine("The following files are in both folders and therefore will be deleted:");
               foreach (var v in queryCommonFiles)
               {
                  Console.WriteLine(v.FullName); //shows which items will be deleted
                  v.Delete(); 
               }
            }
            else
               Console.WriteLine("There are no common files in the two folders.");
            Console.WriteLine("");

            //// Find the set difference between the two folders. For this example we only check one way. 
            //var queryList1Only = (from file in list1
            //                      select file).Except(list2, myFileCompare);
            //Console.WriteLine("The following files are in list1 but not list2:");
            //foreach (var v in queryList1Only)
            //   Console.WriteLine(v.FullName);
            //Console.WriteLine("");

         }
         catch (Exception e)
         {
            LogException(e);
            strFinalResult = "Error: See log";
         }

         // Keep the console window open.
         Console.WriteLine(strFinalResult);
         Console.WriteLine("Press any key to exit.");
         Console.ReadKey();
      }


      private static void LogException(Exception e)
      {
         string strLastExceptionMessage = e.Message;
         string strExceptionStackTrace = e.StackTrace;
         Exception eInnerException = e.InnerException;
         System.Text.StringBuilder msg = new System.Text.StringBuilder("----An error occured----\r\n");
         msg.Append(e.Message);
         while (eInnerException != null)
         {
            if (strLastExceptionMessage != eInnerException.Message)
            {
               strLastExceptionMessage = eInnerException.Message;
               msg.AppendFormat("\r\n\r\n----Inner error----\r\n{0}", strLastExceptionMessage);
            }
            strExceptionStackTrace = eInnerException.StackTrace;
            eInnerException = eInnerException.InnerException;
         }
         msg.AppendFormat("\r\n\r\n----Stacktrace----\r\n{0}", strExceptionStackTrace);
         StreamWriter sw = new StreamWriter(string.Format("{0}{1}_Error.txt", _strApplicationPath, System.DateTime.Now.ToString("yyyyMMdd_HHhmmss")));
         sw.Write(msg.ToString());
         sw.Close();
         sw.Dispose();
      }
   }



   // This implementation defines a very simple comparison 
   // between two FileInfo objects. It only compares the name 
   // of the files being compared and their length in bytes. 
   class FileCompare : System.Collections.Generic.IEqualityComparer<FileInfo>
   {
      public FileCompare() { }

      public bool Equals(FileInfo f1, FileInfo f2)
      {
         return (f1.Name == f2.Name &&
                 f1.Length == f2.Length);
      }

      // Return a hash that reflects the comparison criteria. According to the  
      // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must 
      // also be equal. Because equality as defined here is a simple value equality, not 
      // reference identity, it is possible that two or more objects will produce the same 
      // hash code. 
      public int GetHashCode(FileInfo fi)
      {
         string s = String.Format("{0}{1}", fi.Name, fi.Length);
         return s.GetHashCode();
      }
   }
}
