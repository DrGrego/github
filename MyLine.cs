using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VectorRedactor
{
    class Line: Figure // класс ОТРЕЗОК
    {
        // минимальная допустимая длина линии
        static int MinLength = 30;

        // максимальное допустимое расстояние промаха по отрезку
        // при выборе мышью
        static int MaxMissclick = 4;

        Point P1, P2; // концы отрезка

        // конструктор класса
        public Line(Point P1, Point P2,
            Color ClrPen, int penWidth, DashStyle ds) :
            // Color.Empty: для создания "фиктивной" кисти -
            // для отрезка кисть не требуется
            base(Color.Empty, ClrPen, penWidth, ds)
        {
            this.P1 = P1;
            this.P2 = P2;
        }

        public override void Paint(Graphics g)
        {
            // рисование линии
            g.DrawLine(p, P1, P2);
        }

        // метод перемещения заданной точки
        void MovePoint(ref Point p, int dx, int dy)
        {
            p.X += dx;
            p.Y += dy;
        }

        public override void Move(int dx, int dy)
        {
            // переместить отрезок = переместить его концы
            MovePoint(ref P1, dx, dy);
            MovePoint(ref P2, dx, dy);
        }

        // расстояние между двумя точками
        double dist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public override void Resize(int dx, int dy)
        {
            MovePoint(ref P2, dx, dy);
        }

        // получение общего уравнения прямой для отрезка
        void GetABC(ref double A, ref double B, ref double C)
        {
            // Ax+By+C=0

            // возможно, реально обойтись и без отдельного условия
            // для вертикального отрезка, но я подстраховался
            if (P1.X == P2.X)
            {
                A = 1;
                C = -P1.X;
                B = 0; // y не представлен в уравнении
            }
            else // не вертикальный отрезок
            {
                // выводится в курсе алгебре и геометрии
                A = P2.Y - P1.Y;
                B = P1.X - P2.X;
                C = -P1.X * P2.Y + P2.X * P1.Y;
            }
        }

        // расстояние от точки до прямой Ax+By+C=0
        double DistToLine(double x0, double y0, double A, double B, double C)
        {
            return Math.Abs(A * x0 + B * y0 + C) / Math.Sqrt(A * A + B * B);
        }

        // расстояние от точки до прямой нашего отрезка
        double DistToLineOfPiece(double x, double y)
        {
            double A = 0, B = 0, C = 0;

            // сначала ищем уравнение прямой
            GetABC(ref A, ref B, ref C);

            // теперь ищем расстояние от точки до прямой
            return DistToLine(x, y, A, B, C);
        }

        // пересечение двух прямых вида y=kx+b
        // вызываем только при гарантии, что пересечение есть
        void Intersection(double k1, double b1, double k2, double b2, double[] result)
        {
            result[0] = (b2 - b1) / (k1 - k2);
            result[1] = k1 * result[0] + b1;
        }

        // получение проекции точки на прямую отрезка
        double[] GetProjection(double xPoint, double yPoint)
        {
            double[] result = new double[2];

            if (P1.X == P2.X) // для вертикального отрезка
            {
                result[0] = P1.X;
                result[1] = yPoint;
            }
            else
            {
                if (P1.Y == P2.Y) // для горизонтального отрезка
                {
                    result[1] = P1.Y;
                    result[0] = xPoint;
                }
                else // для наклонного
                {
                    // представление прямой отрезка в виде y=kx+b
                    double k1 = (P2.Y - P1.Y) / ((double)(P2.X - P1.X));
                    double b1 = P1.Y - k1 * P1.X;

                    // представление НОРМАЛИ к отрезку в виде y=kx+b
                    double k2 = -1.0 / k1;
                    double b2 = yPoint - k2 * xPoint;

                    // ищем пересечение прямой отрезка и нормали
                    Intersection(k1, b1, k2, b2, result);
                }
            }

            return result;
        }

        // получение t-параметра точки, лежащей на отрезке,
        // отрезок рассматривается в параметрической форме:
        // Начало*(1-t) + Конец*t, 0<=t<=1
        double GetTParam(double x, double y)
        {
            // сначала получаем проекцию точки на отрезок
            double[] proj = GetProjection(x, y);

            // теперь определяем t
            if (Math.Abs(P2.X - P1.X) > Math.Abs(P2.Y - P1.Y))
                // целочисленного деления тут не будет, хотя знаменатель целый:
                // в числителе double минус целое = double
                return (proj[0] - P1.X) / (P2.X - P1.X);
            return (proj[1] - P1.Y) / (P2.Y - P1.Y);
        }

        // содержит ли отрезок заданную точку?
        public override bool Contains(int x, int y)
        {
            // считаем расстояние от точки до прямой, и если оно
            // выше допустимого, уходим
            if (DistToLineOfPiece(x, y) > MaxMissclick)
                return false;

            // определяем координаты проекции точки на отрезок
            double[] proj = GetProjection(x, y);

            // при параметрическом представлении отрезка точке proj
            // должно соответствовать какое-то t при 0 <= t <= 1
            double t = GetTParam(x, y);
            return (t >= 0 && t <= 1);
        }

        // переопределение метода IsValid
        public override bool IsValid()
        {
            return (dist(P1.X, P1.Y, P2.X, P2.Y) >= MinLength);
        }
        public override string SaveToSVG()
        {
            string clrStroke = ColorTranslator.ToHtml(p.Color);
            string str = $"<line x1=\"{P1.X}\" y1=\"{P1.Y}\" x2=\"{P2.X}\" y2=\"{P2.Y}\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\"";
            if (p.DashStyle == DashStyle.Solid) str += "";
            if (p.DashStyle == DashStyle.Dash)
            {
                if (p.Width < 3) str += "stroke-dasharray=\"5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width * 2},{p.Width * 2}\" ";
            }
            if (p.DashStyle == DashStyle.DashDot)
            {
                if (p.Width < 3) str += "stroke-dasharray=\"10,5,5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width * 4},{p.Width * 2},{p.Width * 2},{p.Width * 2}\" ";
            }
            str += "/>";
            return str;
        }
    }
}