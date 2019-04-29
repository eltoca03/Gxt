﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Gxt.ElevationProfileDesigner
{
    public class ProfileGrid
    {
        private int GridLength { get; set; }
        private int GridOffset { get; set; }
        private int HorizontalLineCount { get; set; }
        public DBObjectCollection ProfileGridDBOjbects { get; set; }
        public Point3d InsertionPoint { get; set; }

        Document doc = Application.DocumentManager.MdiActiveDocument;

        //default length of grid 
        public ProfileGrid()
        {
            GridLength = 250;
            GridOffset = 5;
            ProfileGridDBOjbects = new DBObjectCollection();
            Init();
        }

        public ProfileGrid(int l, int offset)
        {
            GridLength = l + 30; //add 30 for buffer on the grid side 15 on each side
            GridOffset = offset;
            HorizontalLineCount = 30;
            ProfileGridDBOjbects = new DBObjectCollection();
            Init();
        }

        public void Init()
        {
            Line line;
            Editor ed = doc.Editor;

            PromptPointOptions ppo = new PromptPointOptions("\nSelect profile insertion point: ");
            PromptPointResult ppr = ed.GetPoint(ppo);

            if (ppr.Status != PromptStatus.OK)
            {
                return;
            }

            InsertionPoint = ppr.Value;

            Point3d endGridPt = new Point3d(InsertionPoint.X + this.GridLength, InsertionPoint.Y, InsertionPoint.Z);

            line = new Line(InsertionPoint, endGridPt);

            //Horizontal grid lines
            for (int i = 0; i <= HorizontalLineCount ; i++)
            {
                if (i == 0)
                    this.ProfileGridDBOjbects.Add(line.GetOffsetCurves(0)[0]);

                if (i % 5 == 0)
                {
                    line.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 255, 255);
                    this.ProfileGridDBOjbects.Add(line.GetOffsetCurves(i * GridOffset)[0]);
                }
                else
                {
                    line.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(80, 80, 80);
                    this.ProfileGridDBOjbects.Add(line.GetOffsetCurves(i * GridOffset)[0]);
                }
            }

            //verticle grid lines every 100 feet
            Point3d vpointBottom = new Point3d(InsertionPoint.X + 15, InsertionPoint.Y, InsertionPoint.Z);
            Line vline = new Line(vpointBottom, new Point3d(vpointBottom.X, vpointBottom.Y + (GridOffset * HorizontalLineCount), vpointBottom.Z));

            for (int i = 0; i <= (int)(GridLength / 100); i++)
            {
                this.ProfileGridDBOjbects.Add(vline.GetOffsetCurves(-(i * 100))[0]);
            }

            PlaceGridElevationText();
            PlaceProfileStationText();
        }

        private void PlaceProfileStationText()
        {
            MText mText = new MText()
            {
                Contents = FormatStation(0),
                TextHeight = 2.5,
                Location = new Point3d(InsertionPoint.X + 15, InsertionPoint.Y + (GridOffset * HorizontalLineCount), InsertionPoint.Z),
                Attachment = AttachmentPoint.BottomCenter
            };
            

            ProfileGridDBOjbects.Add(mText);

            for (int i = 1; i <= (GridLength / 100); i++)
            {
                MText m = (MText)mText.Clone();
                m.Contents = FormatStation(i * 100);
                m.Location = new Point3d(mText.Location.X + (i * 100), mText.Location.Y, mText.Location.Z);
                ProfileGridDBOjbects.Add(m);
            }
        }

        public void PlaceGridElevationText()
        {
            MText mText = new MText(){
                TextHeight = 2.5,
                Location = new Point3d(InsertionPoint.X - 5, InsertionPoint.Y, InsertionPoint.Y),
                Contents = "00"
            };

            for (int i = 0; i <= HorizontalLineCount / 5; i++)
            {
                MText mt = (MText)mText.Clone();
                mt.Contents = (i * 5).ToString();
                mt.Location = new Point3d(mText.Location.X, mText.Location.Y + (GridOffset * i* 5), mText.Location.Z);
                ProfileGridDBOjbects.Add(mt);
            }

            MText mTextEnd = (MText)mText.Clone();
            mTextEnd.Location = new Point3d(InsertionPoint.X + (GridLength + 5), InsertionPoint.Y, InsertionPoint.Z);
           
            for (int i = 0; i <= HorizontalLineCount / 5; i++)
            {
                MText mt = (MText)mTextEnd.Clone();
                mt.Contents = (i * 5).ToString();
                mt.Location = new Point3d(mTextEnd.Location.X, mTextEnd.Location.Y + (GridOffset * i * 5), mTextEnd.Location.Z);
                ProfileGridDBOjbects.Add(mt);
            }
        }

        public string FormatStation(int v)
        {
            string formattedStation = "";

            if (v.ToString().Length == 1)
            {
                formattedStation = "0+0" + v.ToString();
            }
            else if (v.ToString().Length == 2)
            {
                formattedStation = "0+" + v.ToString();
            }
            else
            {
                char[] array = v.ToString().ToCharArray();

                for (int i = 0; i < array.Length; i++)
                {
                    if (array.Length - i == 2)
                    {
                        formattedStation += "+";
                    }

                    formattedStation += array[i];
                }

            }
            return formattedStation;
        }
    }
}