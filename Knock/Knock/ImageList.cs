﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Knock
{
    // list of Received images.
    public class ImageList : ObservableCollection<ImagesItems> 
    {
        public ImageList GetImageList() { return this; }
    }

    // list of shared images.
    public class MyList : ObservableCollection<My>
    {

    }
}
