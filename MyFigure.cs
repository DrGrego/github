using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VectorRedactor
{

    abstract class Figure // базовый класс ФИГУРА
    {
        protected SolidBrush b; // кисть для заливки
        protected Pen p; // перо для контура

        // true, если контур не надо показывать
        protected bool IsStealthPen;

        // минимальный допустимый размер, например,
        // мин радиус круга
        public static int BoundarySize = 10;

        // установка цвета кисти
        public void SetBrushColor(Color ClrBrush)
        {
            this.b = new SolidBrush(ClrBrush);
        }

        // установка параметров пера
        public void SetPen(Color ClrPen, int penWidth, DashStyle ds)
        {
            this.p = new Pen(ClrPen, penWidth);
            this.p.DashStyle = ds;
            this.IsStealthPen = false;
        }

        // установка требования не рисовать контур
        public void SetStealthPen()
        {
            this.IsStealthPen = true;
        }

        // конструктор класса
        public Figure(Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds)
        {
            SetBrushColor(ClrBrush);
            SetPen(ClrPen, penWidth, ds);
            IsStealthPen = false;
        }

        // абстрактный метод отрисовки
        public abstract void Paint(Graphics g);

        // перемещение (dx, dy - смещение по осям)
        public abstract void Move(int dx, int dy);

        // смена размеров
        public abstract void Resize(int dx, int dy);

        // включает ли фигура точку?
        public abstract bool Contains(int x, int y);

        // найти индекс последней фигуры, содержащей точку (x, y), в списке lst
        public static int FindPt(int x, int y, List<Figure> lst)
        {
            for (int i = lst.Count - 1; i >= 0; i--)
                if (lst[i].Contains(x, y) == true)
                    return i;
            return -1;
        }

        // отрисовать все фигуры из списка lst
        public static void DrawList(List<Figure> lst, Graphics g)
        {
            foreach (Figure item in lst)
                item.Paint(g);
        }

        // перевод радиан в градусы
        protected double RadToGrad(double fi)
        {
            return fi * 180.0 / Math.PI;
        }

        // перевод градусов в радианы
        protected double GradToRad(double fi)
        {
            return fi * Math.PI / 180.0;
        }

        // является ли фигура допустимой?
        public virtual bool IsValid()
        {
            return true;
        }
        //генерация svg строки
        public abstract string SaveToSVG();
        
    }
}