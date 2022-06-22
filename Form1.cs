using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace VectorRedactor
{
    enum DrawMode // текущий режим
    {
        None, // ничего не делается
        Moving, // перемещается некоторая фигура
        Creating, // создаётся фигура
    }

    public partial class Form1 : Form
    {
        bool saved=false;//сохранён ли файл
        DrawMode dm; // храним текущий режим рисования

        // предыдущее положение курсора
        int prev_x, prev_y;

        // индекс выбранной фигуры в списке (например, выбранной для перемещения мышью)
        int c_index = -1;

        // коллекция фигур
        List<Figure> figures = new List<Figure>();

        // для запоминания, какой стиль линии у создаваемой фигуры
        DashStyle ds = DashStyle.Solid;

        // названия команд
        string[] ItemsNames = { "Круг", "Прямоугольник", "Квадрат", "Сектор",
            "Многоугольник", "Отрезок", "Очистить" };

        // номер выбранной команды
        int SelTypeIndex = -1;

        // получение файла изображения для кнопки
        string GetImgFile(string fName, bool IsLighting = false)
        {
            if (IsLighting == true) fName = "light_" + fName;
            return "img\\" + fName + ".png";
        }

        // добавление кнопки с заданным именем по заданной позиции
        void AddItem(string itemName)
        {
            toolStrip1.Items.Add(new ToolStripButton(itemName));

            int p = toolStrip1.Items.Count - 1;

            // текст подсказки при наведении курсора
            toolStrip1.Items[p].ToolTipText = itemName;

            // отображение кнопки тулбара - показывать только изображение
            toolStrip1.Items[p].DisplayStyle = ToolStripItemDisplayStyle.Image;

            // выбрать изображение
            toolStrip1.Items[p].Image = Image.FromFile(GetImgFile(itemName));
        }

        public Form1()
        {
            InitializeComponent();
            dm = DrawMode.None; // пока что ничего не рисуем

            // выбираем верхний элемент комбобокса
            comboBox1.SelectedIndex = 0;

            // добавляем кнопки на тулбар
            foreach (string itemName in ItemsNames)
                AddItem(itemName);
        }

        // перенос index-ой фигуры в хвост списка
        void FigureToFront(int index)
        {
            Figure temp = figures[index];
            figures.RemoveAt(index); // удалить фигуру
            figures.Add(temp); // вставить ее в конец
            saved = false;
        }

        // добавление фигуры по заданной позиции
        void Addition(int X, int Y)
        {
            // выбранная толщина контура
            int penW = (int)numericUpDown1.Value;

            // действуем в зависимости от типа фигуры
            switch (ItemsNames[SelTypeIndex])
            {
                case "Круг":
                    // добавляем фигуру в список;
                    // запоминаем позицию, размеры фигуры и параметры
                    // контура, заливки
                    figures.Add(new Circle(X, Y, Figure.BoundarySize,
                        pbColor.BackColor, pbColorBound.BackColor, penW, ds));
                    break;
                case "Прямоугольник":
                    figures.Add(new Rectangle(X, Y, Figure.BoundarySize, Figure.BoundarySize,
                        pbColor.BackColor, pbColorBound.BackColor, penW, ds));
                    break;
                case "Квадрат":
                    figures.Add(new Square(X, Y, Figure.BoundarySize,
                        pbColor.BackColor, pbColorBound.BackColor, penW, ds));
                    break;
                case "Сектор":
                    figures.Add(new Pie(X, Y, Figure.BoundarySize,
                        (double)numStartAngle.Value, (double)numEndAngle.Value,
                        pbColor.BackColor, pbColorBound.BackColor, penW, ds));
                    break;
                case "Многоугольник":
                    figures.Add(new TrueNAngle(X, Y, Figure.BoundarySize,
                        (int)numPolyN.Value, (double)numPolyAngle.Value,
                        pbColor.BackColor, pbColorBound.BackColor, penW, ds));
                    break;
                case "Отрезок":
                    {
                        Point p = new Point(X, Y);
                        figures.Add(new Line(p, p, pbColorBound.BackColor, penW, ds));
                    }
                    break;
            }
            saved = false;
        }

        // функция реакции на нажатие левой кнопкой мыши по PictureBox
        void OnLeftClick(MouseEventArgs e)
        {
            if (dm == DrawMode.None) // если ничего не создаётся и не перемещается
            {
                // ищем индекс выбираемой фигуры
                c_index = Figure.FindPt(e.X, e.Y, figures);
                if (c_index == -1) // если у нас не выбрана никакая фигура
                {
                    // если у нас не выбран тип фигуры
                    if (SelTypeIndex == -1)
                    {
                        MessageBox.Show("Выберите тип добавляемой фигуры с помощью тулбара");
                        return;
                    }

                    // если выбран тип фигуры, добавляем ее
                    Addition(e.X, e.Y);

                    // переходим в режим создания
                    dm = DrawMode.Creating;
                }
                // если выбрана какая-то фигура, перемещаем
                // ее на передний план
                else FigureToFront(c_index);

                // теперь выбранная фигура последняя в списке
                // (как при создании, так и при выборе
                // существующей фигуры ЛКМ)
                c_index = figures.Count - 1;

                // если мы не были в режиме создания,
                // переходим в режим перемещения
                if (dm == DrawMode.None) dm = DrawMode.Moving;

                // запоминаем, что движение мышью еще не началось
                prev_x = -1;

                pictureBox1.Refresh();
            }
        }

        // при нажатии кнопки мыши над PictureBox
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // если левой кнопкой
                OnLeftClick(e);
            if (e.Button == MouseButtons.Right) // если правой кнопкой
            {
                if (dm == DrawMode.None) // если ничего не создаётся и не перемещается
                {
                    // проверяем, не выбрана ли какая-то фигура
                    c_index = Figure.FindPt(e.X, e.Y, figures);

                    // если ничего не выбрано
                    if (c_index == -1) return;

                    // иначе выводим контекстное меню
                    contextMenuStrip1.Show(this, this.PointToClient(Cursor.Position));

                    // запрещаем пункт невидимого контура для линии
                    mStealth.Enabled = !(figures[c_index] is Line);
                    // то же про пункт заливки
                    mFill.Enabled = mStealth.Enabled;
                }
            }
        }

        // когда водим курсором над PictureBox
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // если мы ничего не создаём и не перемещаем, не нужно запоминать позицию курсора мыши
            if (dm == DrawMode.None) return;

            if (prev_x != -1 && c_index != -1)
            {
                // разница новых и старых координат курсора
                int dx = e.X - prev_x;
                int dy = e.Y - prev_y;

                // если мы в режиме перемещения
                if (dm == DrawMode.Moving)
                    figures[c_index].Move(dx, dy);
                // если мы в режиме создания
                else figures[c_index].Resize(dx, dy);

                pictureBox1.Refresh();
                saved = false;
            }

            // новые координаты теперь становятся старыми
            prev_x = e.X;
            prev_y = e.Y;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Figure.DrawList(figures, e.Graphics);
        }

        // при смене выбранного элемента в раскрывающемся списке
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DashStyle[] vars = { DashStyle.Solid, DashStyle.Dash, DashStyle.DashDot };
            ds = vars[comboBox1.SelectedIndex];
        }

        // при отпускании кнопки мыши, нажатой над PictureBox
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // если нажатие было не левой кнопкой или мы в
            // режиме "ничего неделания"
            if (dm == DrawMode.None || e.Button != MouseButtons.Left)
                return;

            // если мы были в процессе создания фигуры
            if (dm == DrawMode.Creating)
            {
                // индекс добавленной фигуры
                int n = figures.Count - 1;

                // если у добавленной фигуры некорректные параметры
                if (!figures[n].IsValid())
                    // удаляем фигуру
                    figures.RemoveAt(n);
            }
            dm = DrawMode.None;
            pictureBox1.Refresh();
        }

        // при выборе "Применить новый стиль" в контекстном меню
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dm != DrawMode.None) return;
            // вызываем метод изменения пера для контура
            figures[c_index].SetPen(pbColorBound.BackColor, (int)numericUpDown1.Value, ds);
            saved = false;
            pictureBox1.Refresh();
        }

        // функция получения цвета из стандартного диалога
        Color GetColor()
        {
            // пользуемся стандартным диалогом для выбора цвета
            ColorDialog dia = new ColorDialog();
            dia.AllowFullOpen = true;
            dia.AnyColor = true;

            // если пользователь нажал ОК в диалоговом окне
            if (dia.ShowDialog() == DialogResult.OK) return dia.Color;

            // если выбор отменен
            return Color.Empty; // признак незаданного цвета
        }

        // при выборе "Изменить цвет заливки" в контекстном меню
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Color clr = GetColor();
            if (clr == Color.Empty) return; // если выбор цвета отменен
            // изменяем цвет заливки фигуры
            figures[c_index].SetBrushColor(clr);
            pictureBox1.Refresh();
            saved = false; 
        }

        // задание цвета фона picturebox-а
        void SetColorForPicture(PictureBox pBox)
        {
            Color clr = GetColor();
            if (clr == Color.Empty) return;
            pBox.BackColor = clr;
        }

        // при щелчках по маленьким PictureBox
        private void pbColorBound_Click(object sender, EventArgs e)
        {
            SetColorForPicture(pbColorBound);
        }
        private void pbColor_Click(object sender, EventArgs e)
        {
            SetColorForPicture(pbColor);
        }

        private void сделатьКонтурНевидимымToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // вызываем метод установки пера как невидимого
            figures[c_index].SetStealthPen();
            pictureBox1.Refresh();
        }

        // функция подсвечивает p-ую кнопку тулбара
        void SetLightingIndex(int p)
        {
            for (int i = 0; i < ItemsNames.Length; i++)
            {
                string itemName = toolStrip1.Items[i].Text;

                // подсветить кнопку = загрузить изображение, отличное
                // от выводимого в обычном состоянии
                toolStrip1.Items[i].Image = Image.FromFile(GetImgFile(ItemsNames[i], i == p));
            }
        }

        // при нажании на кнопки тулбара
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // номер нажатой кнопки
            int p = Array.IndexOf(ItemsNames, e.ClickedItem.Text);

            // если это не последняя кнопка (т.е. не кнопка очистки)
            if (p != ItemsNames.Length - 1)
            {
                SelTypeIndex = p; // запомнить тип фигуры для добавления
                SetLightingIndex(p); // подсветить выбранную кнопку
            }
            else // если это последняя кнопка
            {
                figures.Clear();
                pictureBox1.Refresh();
                saved=false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Save();
        }

        // при выборе "Удалить" в контекстном меню
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // удаление выбранной фигуры из списка
            figures.RemoveAt(c_index);
            pictureBox1.Refresh();
            saved = false;
        }

        //сохранение файла при закрытии приложения если файл не был сохранён ранее
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(figures.Count > 0 && !saved)
            {
                e.Cancel = true;
                Save();
            }
            e.Cancel = false;
        }

        private void Save()
        {
            //сохранение в svg
            string strMsg = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" + "\r\n" + "<svg xmlns = \"http://www.w3.org/2000/svg\" version = \"1.1\" height = \"404px\"  width = \"802px\" >" + "\r\n";
            foreach (Figure item in figures)
            {
                strMsg += item.SaveToSVG();

                strMsg += Environment.NewLine; // перевод строки
            }
            strMsg += "</svg>";
            SaveFileDialog save = new SaveFileDialog(); //вызов диалога для сохранения файла

            save.FileName = "unnamed.svg"; //стандартное имя файла

            save.Filter = "svg file | *.svg"; //расширение файла

            if (save.ShowDialog() == DialogResult.OK)

            {

                StreamWriter writer = new StreamWriter(save.OpenFile());

                writer.Write(strMsg);

                writer.Close();

            }
            saved=true;
        }

    }
}