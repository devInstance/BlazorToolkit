using System;
using System.Collections.Generic;
using System.Linq;

namespace DevInstance.BlazorToolkit.Components;

internal static class DataPageUtils
{
    public static IEnumerable<int> GetPageRange(int selectedPage, int pageCount, int maxItems)
    {
        if (pageCount <= maxItems)
        {
            return Enumerable.Range(0, pageCount);
        }

        // default position for the selected item:
        //        V
        // [0][1][2][3][4]
        int midPosition = (maxItems / 2) - 1;
        // check if selected page is "close" to start
        // and cannot "sit" in the middle
        int realPosition = Math.Min(selectedPage, midPosition);

        // check if selected page is "close" to start
        // and cannot "sit" in the middle
        int endMargin = pageCount - midPosition;
        if (selectedPage > endMargin)
        {
            realPosition = maxItems - (pageCount - selectedPage - 1) - 1;
        }

        var range = new int[maxItems];
        range[0] = 0;//first page
        range[maxItems-1] = pageCount - 1; //last page
        //populate page numbers
        //1. moving backward
        for(int i = realPosition, n = 0; i >= 1; i --, n ++)
        {
            range[i] = selectedPage - n;
        }
        //2. moving forward
        for (int i = realPosition + 1, n = 1; i < maxItems - 1; i ++, n++)
        {
            range[i] = selectedPage + n;
        }
        return range;
    }
}


