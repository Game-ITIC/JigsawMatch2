// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("KBybHZiDL9kJIwbEbj+AyyUzqSdeNyyWIrv3lVOPb71z7cnuVeg+aGVH52pRJPYslu7bfsaKGcGa1RQNpBaVtqSZkp2+EtwSY5mVlZWRlJddDaPWJAcIxOzKhrT6RLuNg5qLg/LLtEbldlGICOwTqBQ3AVxEUfOBFsfWyWH4NPZfYJZ6NysjhmPnshYAqpVL4uWMib4V/hOAV653tbF0tlIQ2fpF9WoAYR3QNTeNRaR0Key7ECLdhMVfd/e9n4UTQHf3mzSG1ezabEFY5EKwln5NrUQrcUOi9S9raVMidOTNnn+uPDK54Zk2vv0FxyIwSrtnvbQIe03d70OMWHpRiGt1tz8WlZuUpBaVnpYWlZWUN18MGXLKz5ObSIFe52qzM5aXlZSV");
        private static int[] order = new int[] { 6,3,11,6,7,12,12,11,9,9,10,11,13,13,14 };
        private static int key = 148;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
