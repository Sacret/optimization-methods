using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //
        //________________________________________________________________________________
        //                          ПЕРЕМЕННЫЕ И КОНСТАНТЫ
        //
          int     n = 3;                 // размерность пространства
          int     RB = 0;                // значение нажатия radiobutton в методах
          int     RB1 = 0;               // значение нажатия radiobutton в графике
          float   s = 0.25F;             // значение шага        
          float[] x;                     // координаты точки
          float   y;                     // значене функции
          float[] a = new float[1000];   // массив, хранящие все полученные точки
          int     l = 0;                  // количество этих точек
        //
        //________________________________________________________________________________
        //                          ФУНКЦИИ
        //
        // Минимизируемая функция 
        //      Вход:  вектор координат x
        //      Выход: значение функции в этой точке
        float fun(float[] x)
        {
            // для условной оптимизации
            if (RB == 3) return (x[0] - 16) * (x[0] - 16) + (x[1] - 9) * (x[1] - 9);
            // для безусловной оптимизации
            else return 4 * x[0] * x[0] + 5 * x[1] * x[1] + 7 * x[2] * x[2] - 2 * x[0] * x[1] + x[0] * x[2] + x[0] - x[1] + x[2];
        }
        //________________________________________________________________________________
        // Градиент - для ФР
        //      Вход: вектор координат x, вектор-градиент g
        //      Выход: значение градиента записывается в массив g
        void grad (float[] x, float[] g)
        {
           g[0]=8*x[0]-2*x[1]+x[2]+1;
           g[1]=10*x[1]-2*x[0]-1;
           g[2]=14*x[2]+x[0]+1;
           return;
        }      
        //________________________________________________________________________________
        // Расчет фунуции условной оптимизации
        //      Вход:  вектор координат x, вектор направлений d, значения лямбда lm, значение mu
        //      Выход: значение функции в полученной точке
        float cf(float[] x, float[] d, float lm, float mu)
        {
            int i;
            float[] x1 = new float[n];
            //
            for (i = 0; i < n; i++)
                x1[i] = x[i] + lm * d[i];
            if (mu!=0) return p(x1, mu);
              else return fun(x1);
        }
        //________________________________________________________________________________              
        // Функция штрафа
        //      Вход:  вектор координат x
        //      Выход: значение штрафа в этой точке
        float alfa (float[] x)
        {
            float a = 0;
            if (5*x[0]-2*x[1]-60>=0) a+=5*x[0]-2*x[1]-60;
            if (x[0]+x[1]-15>=0)     a+=x[0]+x[1]-15;
            if (x[0]+4*x[1]-40>=0)   a+=x[0]+4*x[1]-40;
            return a;
        }
        //________________________________________________________________________________
        // Функция условной оптимизации (для штрафных функций)
        //      Вход:  вектор кординат x, значение mu
        //      Выход: значение функции в этой точке
        float p(float[] x, float mu)
        {
            float y;
            y = fun(x) + alfa(x) * mu;
            return y;
        }
        //________________________________________________________________________________     
        // Золотое сечение
        //      Вход:  вектор координат x, вектор направлений d, значение начала отрезка a,
        //             значение конца отрезка b, точность eps, значение mu
        //      Выход: точка минимума на заданном интервале
        float ZolSec (float[] x, float[] d, float a, float b, float eps, float mu)
        {
            float i, k, x1, x2, f1, f2;            
            k = 0.618F;          
            i = b - a;
            x1 = a + (1 - k) * i;
            x2 = a + k * i;
            f1 = cf(x, d, x1, mu);
            f2 = cf(x, d, x2, mu);           
            while (Math.Abs(x2 - x1) > eps)
            {
                if (f1 > f2)
                {                    
                    a = x1;                    
                    x1 = x2;
                    x2 = a + k * (b - a);
                    f1 = f2;
                    f2 = cf(x, d, x2, mu);
                }
                else
                {                    
                    b = x2;
                    x2 = x1;
                    x1 = a + (1 - k) * (b - a);
                    f2 = f1;
                    f1 = cf(x, d, x1, mu);
                }
            }
            return (x1 + x2) / 2;
        }
        //________________________________________________________________________________        
        // Первоначальный интервал (для ЦПС и штрафных функций)
        //      Вход:  вектор координат x, вектор направлений d, значение шага s, значение начала отрезка a,
        //             значение конца отрезка с, значение mu
        //      Выход: в переменные a и c записываются значения интервала
        void interval (float[] x, float[] d, float s, ref float a, ref float c, float mu)
        {
            float f1, f2, f3, b;
            a = -s;
            b = 0;
            c = +s;
            f1 = cf(x, d, a, mu);
            f2 = cf(x, d, b, mu);
            f3 = cf(x, d, c, mu);
            while (!((f1 > f2) && (f3 > f2)))
            {
                if (f2 > f3)
                {
                    a = b;
                    b = c;
                    c = c + s;
                    f1 = f2;
                    f2 = f3;
                    f3 = cf(x, d, c, mu);
                    continue;
                }
                if (f2 > f1)
                {
                    c = b;
                    b = a;
                    a = a - s;
                    f3 = f2;
                    f2 = f1;
                    f1 = cf(x, d, a, mu);
                    continue;
                }
                break;
            }
        }
        //________________________________________________________________________________ 
        // Первоначальный интервал (для ФР)
        //      Вход:  вектор координат x, вектор направлений d, значение шага s, значение начала отрезка a,
        //             значение конца отрезка b, значение точности eps, значение mu
        //      Выход: в переменные a и c записываются значения интервала
        void interval1 (float[] x, float[] d, float s, ref float a, ref float b, float eps, float mu)
        {
          float f1, f2, f3, xm, x1, x2, L;
          xm = (b-a)/2;
          f3 = cf (x, d, xm, mu);
          L=b-a;
          do
          {
            x1 = a+L/4;
            x2 = b-L/4;
            f1= cf (x, d, x1, mu);
            f2= cf (x, d, x2, mu);
            if (f1>=f3)
            {
              if (f2<f3)
              {
	        a = xm;
	        xm = x2;
	        f3 = f2;
              }
              else
              {
	        a = x1;
	        b = x2;
              }
            }
            else
            {
              b = xm;
              xm = x1;
              f3 = f1;
            }
            L = b - a;
          }
          while (Math.Abs(L)>=eps);
          a = xm-s;
          b = xm+s;
        }
        //________________________________________________________________________________ 
        // Квадрат расстояния между точками
        //      Вход:  вектор координат х, вектор координат xs
        //      Выход: квадрат расстояния между ними
        float KvS (float[] x, float[] xs)
        {
            float h = 0;
            int i;
            //
            for (i = 0; i < n; i++)
                h = h + (x[i] - xs[i]) * (x[i] - xs[i]);
            return h;
        }
        //________________________________________________________________________________ 
        // Квадрат нормы вектора
        //      Вход:  вектор координат x
        //      Выход: значение квадрата его нормы
        float NvS (float[] x)
        {
            float h = 0;
          int i;
          //
          for (i=0;i<n;i++)
            h = h + x[i]*x[i];
          return h;
        }
        //________________________________________________________________________________ 
        // Циклический покоординатный спуск
        //      Вход:  вектор координат х, значение шага s, точность eps, значение mu
        //      Выход: вектор x - точка минимума функции
        void CPS (float[] x, float s, float eps, float mu)
        {
            float[] xs = new float[n];
            float[] y = new float[n];            
            float[] d = new float[n];           
            //
            float lm, lm1 = 0, lm2 = 0;
            double eps2 = 0;
            int i, j;           
            for (i = 0; i < n; i++) { a[l] = x[i]; l++; }
            eps2 = eps * eps;
            //
            while (eps2 >= eps * eps)
            {
                for (i = 0; i < n; i++)
                {
                    y[i] = x[i];
                    xs[i] = x[i];
                }                
                j = 0;
                //
                while (j < n)
                {                   
                    for (i = 0; i < n; i++) 
                    {
                        if (i != j) d[i] = 0;
                        else d[i] = 1;
                    }                   
                    interval (x, d, s, ref lm1, ref lm2, mu);
                    lm = ZolSec (x, d, lm1, lm2, eps / n, mu);
                    y[j] = y[j] + lm;
                    // запись в массив для печати графиков
                    for (i = 0; i < n; i++) 
                    {
                        a[l] = y[i];
                        l++;  
                    }
                    j++;                
                }
                //
                for (i = 0; i < n; i++)
                    x[i] = y[i];               
                //
                eps2 = KvS (x, xs);
            }
        }
        //________________________________________________________________________________ 
        // Метод Флетчера-Ривса
        //      Вход:  вектор координат х, значение шага s, точность eps, значение mu
        //      Выход: вектор x - точка минимума функции
        void FR (float[] x, float s, float eps, float mu)
        {
          int j,i;
          float lm, lm1 = 0, lm2 = 0, al, nm1 = 0, nm2 = 0;
          double eps2;
          float[] y  = new float[n];
          float[] d  = new float[n];
          float[] gr = new float[n];
          l = 0;
          for (i = 0; i < n; i++) { a[l] = x[i]; l++; }
          eps2=eps*eps;
          grad(x,gr);
          nm2 = NvS(gr);
          //
          while (nm2 > eps2)
          {            
            nm1 = nm2;
            for (i=0;i<n;i++)
            {
              y[i] = x[i];
              d[i] = - gr[i];
            }
            interval1 (x,d,s,ref lm1,ref lm2,eps,mu);
            lm = ZolSec (x,d,lm1,lm2,eps/n,mu);
            //
            for (i=0;i<n;i++)
              y[i] = y[i] + lm * d[i];
            //
            for(j=0;j<n;j++)
            {
              grad (y,gr);
              nm2 = NvS(gr);
              al  = nm2/nm1;
              for (i=0;i<n;i++)
	          d[i] = -gr[i] + al*d[i];
              nm1=nm2;
              //              
              interval1 (x,d,s,ref lm1,ref lm2,eps,mu);
              lm = ZolSec (x,d,lm1,lm2,eps/n,mu);
              //
              //
              for (i=0;i<n;i++)
	            y[i] = y[i] + lm*d[i];
              for (i = 0; i < n; i++)
              {
                  a[l] = y[i];
                  l++;
              }
            }
            //
            for (i=0;i<n;i++)
              x[i]=y[i];
            //        
            grad (y,gr);
            nm2 = NvS(gr);
          }
        }
        //________________________________________________________________________________ 
        // Метод штрафных функций
        //      Вход:  вектор координат х, значение шага s, точность eps, значение mu
        //      Выход: вектор x - точка минимума функции
        void SHTRAF(float[] x, float s, float eps, float mu)
        {
            float al, b = 10;
            l = 0;
            for (int i = 0; i < n; i++) { a[l] = x[i]; l++; }
            al = alfa(x);
            do
            {
                CPS(x, s, eps, mu);
                mu = mu * b;
                al = alfa(x);
            }
            while ((al > eps) && (mu < 1e+12));
        }        
        //______________________________________________________________________________________
        //                        ФУНКЦИИ НАЖАТИЯ НА КНОПКИ
        //
        // Кнопка "ВЫХОД" в безусловной оптимизации
        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }        
        //_____________________________________________________________________________________   
        // Нажатие radiobutton1
        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {
             RB=1;             
        }
        //_____________________________________________________________________________________   
        // Нажатие radiobutton2
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            RB = 2;            
        }    
        //_____________________________________________________________________________________   
        // Кнопка "РАСЧЁТ" в безусловной оптимизации
        private void button2_Click(object sender, EventArgs e)
        {
            x = new float[n];
            x[0] = float.Parse(textBox1.Text);
            x[1] = float.Parse(textBox2.Text);
            x[2] = float.Parse(textBox3.Text);
            float eps = 0.0001F;  // точность
            l = 0;
                if (RB == 1)
                {
                    CPS(x, s, eps, 0);
                }
                if (RB == 2)
                {
                    FR(x, s, eps, 0);
                }             
                if (RB != 0)
                {
                    y = fun(x);
                    label14.Text = "" + x[0].ToString("N5");
                    label15.Text = "" + x[1].ToString("N5");
                    label16.Text = "" + x[2].ToString("N5");
                    label17.Text = "" + y.ToString("N5");
                }
        }
        //_____________________________________________________________________________________  
        // Кнопка "ВЫХОД" в условной оптимизации
        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //_____________________________________________________________________________________  
        // Кнопка "РАСЧЁТ" в условной оптимизации
        private void button4_Click(object sender, EventArgs e)
        {
            n = 2; // размерность пространства равна 2
            x = new float[n];
            x[0] = float.Parse(textBox4.Text);
            x[1] = float.Parse(textBox5.Text);
            
            float eps = 0.1F;  // точность
            float mu = 0.001F;
            RB = 3;
            SHTRAF(x, s, eps, mu);                
            y = fun(x);
            label23.Text = "" + x[0].ToString("N5");
            label22.Text = "" + x[1].ToString("N5");            
            label20.Text = "" + y.ToString("N5");
            n = 3;
        }
        //_____________________________________________________________________________________  
        // По нажатию на форму - рисование графика
        private void tabPage3_Click_1(object sender, EventArgs e)
        {
            //Задаем цвет и толщину пера:            
            Graphics g = tabPage3.CreateGraphics();
            Pen myPen = new Pen(Color.Black);
            myPen.Width = 1;
            Pen myPen1 = new Pen(Color.Green);
            myPen1.Width = 1;
            Pen myPen2 = new Pen(Color.Red);
            myPen2.Width = 1;
            Pen myPen3 = new Pen(Color.Blue);
            myPen3.Width = 1; 
            //
            int i;            
            float shx;
            float max  = 20;
            float xmax = 450;
            float ymax = 295;
            float midx = 300;
            float midy = 150;
            int midx1=0, midy1=0;
            //      Оси координат
            g.DrawLine(myPen, midx, 5, midx, ymax);
            g.DrawLine(myPen, 150, midy, xmax, midy);
            //      Верхняя стрелочка
            g.DrawLine(myPen, midx, 5, midx + 4, 15);
            g.DrawLine(myPen, midx, 5, midx - 4, 15);            
            //      Правая стрелочка
            g.DrawLine(myPen, xmax, midy, xmax - 10, midy + 4);
            g.DrawLine(myPen, xmax, midy, xmax - 10, midy - 4);
            // 
            if (l != 0)
            {
                max = Math.Abs(a[0]);
                for (i = 1; i < l; i++)
                    if (Math.Abs(a[i]) >= max) max = Math.Abs(a[i]);
            }
            //      Выбор шага разбиения
            shx = midy / max;            
            //      Прорисовка разделений        
            for (i = 0; i < max; i++)
            {
                g.DrawLine(myPen, midx - 1, midy - i * shx, midx + 1, midy - i * shx);
                g.DrawLine(myPen, midx - 1, midy + i * shx, midx + 1, midy + i * shx);
                g.DrawLine(myPen, midx - i * shx, midy - 1, midx - i * shx, midy + 1);
                g.DrawLine(myPen, midx + i * shx, midy - 1, midx + i * shx, midy + 1);
            }            
            //      Рисование графика     
            if (l != 0&&RB1!=0)
            {
                if (RB1 == 1&&RB!=3)
                {
                    for (i = 0; i < l - 3; i += 3)
                    {
                        g.DrawLine(myPen1, midx + a[i] * shx, midy - a[i + 1] * shx, midx + a[i + 3] * shx, midy - a[i + 4] * shx);
                    }                                
                }
                if (RB1 == 1 && RB == 3)
                {
                    for (i = 0; i < l - 2; i += 2)
                    {
                        g.DrawLine(myPen1, midx + a[i] * shx, midy - a[i + 1] * shx, midx + a[i + 2] * shx, midy - a[i + 3] * shx);
                    }
                }
                if (RB1 == 2 && RB!=3)
                {
                    for (i = 1; i < l - 3; i += 3)
                    {
                        g.DrawLine(myPen2, midx + a[i] * shx, midy - a[i + 1] * shx, midx + a[i + 3] * shx, midy - a[i + 4] * shx);
                    }
                }
                if (RB1 == 3 && RB!=3)
                {
                    for (i = 0; i < l - 3; i += 3)
                    {
                        g.DrawLine(myPen3, midx + a[i] * shx, midy - a[i + 2] * shx, midx + a[i + 3] * shx, midy - a[i + 5] * shx);
                    }
                }               
            }
            
        }
        //           
        //___________________________________________________________________________________
        //  Выбор осей координат
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            RB1 = 1; // X Y 
        }
        //
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            RB1 = 2; // Y Z
        }
        //
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            RB1 = 3; // X Z
        }    
                   
    }
}
