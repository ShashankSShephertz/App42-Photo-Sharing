﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Data.Linq.Mapping;


namespace Knock
{
    // create table in local database.
    [Table]
    public class Db
    {
        [Column(IsPrimaryKey = true)]
        public int Id
        {
            get;
            set;
        }
        [Column(CanBeNull = false)]
        public string AccessToken
        {
            get;
            set;
        }
        [Column(CanBeNull = true)]
        public string Name
        {
            get;
            set;
        }
        [Column(CanBeNull = true)]
        public string FbId
        {
            get;
            set;
        }
        [Column(CanBeNull = true)]
        public string Picture
        {
            get;
            set;
        }
        
    }
}
