using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Drawing;

namespace JamCast
{
    public static class BitmapTracker
    {
        private struct TrackerEntry
        {
            public DateTime LastTag;
            public Bitmap Bitmap;
        }

        private static List<TrackerEntry> m_TrackedBitmaps = new List<TrackerEntry>();
        private static object m_Lock = new object();

        public static Bitmap Tag(Bitmap b)
        {
            if (b == null) return b;
            lock (m_Lock)
            {
                if (m_TrackedBitmaps.Count(v => v.Bitmap == b) == 0)
                    m_TrackedBitmaps.Add(new TrackerEntry { LastTag = DateTime.Now, Bitmap = b });
                for (int i = 0; i < m_TrackedBitmaps.Count; i++)
                {
                    var v = m_TrackedBitmaps[i];
                    v.LastTag = DateTime.Now;
                }
            }
            return b;
        }

        public static void Purge()
        {
            lock (m_Lock)
            {
                for (int i = 0; i < m_TrackedBitmaps.Count; i++)
                {
                    var v = m_TrackedBitmaps[i];
                    if ((DateTime.Now - v.LastTag).TotalMilliseconds >= 1000)
                    {
                        v.Bitmap.Dispose();
                        m_TrackedBitmaps.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
