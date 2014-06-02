using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace IMI_Administration
{
    class Exhibit
    {
        #region DECLARATIONS
        private string name;
        private string description;
        private List<Image> images;
        #endregion

        #region CONSTRUCTORS
        public Exhibit()
        {
            this.name = "";
            this.description = "";
            this.images = new List<Image>();
        }

        public Exhibit(string name)
        {
            this.name = name;
            this.description = "";
            this.images = new List<Image>();
        }

        public Exhibit(string name, string description)
        {
            this.name = name;
            this.description = description;
            this.images = new List<Image>();
        }

        public Exhibit(string name, string description, Image image)
        {
            this.name = name;
            this.description = description;
            this.images = new List<Image>();
            this.images.Add(image);
        }

        public Exhibit(string name, string description, List<Image> images)
        {
            this.name = name;
            this.description = description;
            this.images = images;
        }
        #endregion
    }
}
