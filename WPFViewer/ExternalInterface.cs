﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFViewer
{
    class ExternalInterface
    {
        private static ExternalInterface _instance;
        private PDFDocumentDataProcessor _documentData;
        private bool _isNewDocumentLoaded = false;

        public PDFDocumentDataProcessor DocumentData
        {
            get { return _documentData; }
        }

        public System.Collections.Generic.ICollection<System.Windows.Media.FontFamily> FontFamilies
        {
            get { return System.Windows.Media.Fonts.SystemFontFamilies; }
        }

        public bool IsNewDocumentLoaded
        {
            get { return _isNewDocumentLoaded; }
            set
            {
                _isNewDocumentLoaded = value;
            }
        }

        public static ExternalInterface Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ExternalInterface();
                return _instance;
            }
        }

        private ExternalInterface()
        {
            _documentData = new PDFDocumentDataProcessor();
        }

        public void OpenFile(string filePath)
        {
            PDFReader.Document document = new PDFReader.Document(filePath);
            document.LoadPDFDocument();

            _documentData.Clear();
            _documentData.LoadPages(document);
            IsNewDocumentLoaded = true;
            //_documentData.LoadVisiblePages();
        }
    }
}


//  Done. Обрабатывать запросы на перевод через обычный переводчик и предоставлять несколько вариантов
//  Done. Понять, почему иногда нет перевода, исправить это.
//  Canceled. Сделать категории групп слов на том языке, на котором запрашивается перевод.
//  Done. При нажатии на слово и выскакивании подсказки его цвет меняется на синий, пока подсказка не исчезнет
//  Done. После прокрутке переводов подсказка не исчезает после перевода мыши. Исправить
//  Done. Все полученные переводы сохранять в словаре и предоставлять, если снова запрашиваются, без запроса на сайт
//  Canceled. Учитывать возможный перенос слова на следующую строку
//  Explanation. Вместо реализации предыдущего пункта было принято решение добавить возможность объединения блоков без добавления пробелов между ними и удалением "-" по нажатию кнопки shift
//  Done. Возможность объединять слова в выражения по нажатию кнопки ctrl
//  Done. Сделать FontFamilySelectedIndex для видимого диапазона
//  Done. Сделать TextSize для видимого диапазона и подружить с PageSize
//  Almost done. При изменении размера страницы двигать ScrollBar в соответствии с изменениями
//  Explanation. Сделано не очень точно, но работает.
//  Canceled. Сделать более точный рассчет для отображаемых страниц, учитывая расстояние между страницами
//  Explanation. Достаточно точно рассчитывает, нет необходимости уточнять.
//  Canceled. Ускорить подгрузку новых страниц с требуемым параметрами, т.е. сразу подгружать с нужными.
//  Explanation. Подгружается достаточно быстро. Нет необходимости это реализовывать.

//  Canceled. Проверять разные блоки текста на предмет их схожести по размеру текста и если он близок, ставить один и тот же
//  Explanation. Было принято решение не реализовывать данную функцию по причине трудоемкости и предполагаемой относительно небольшой значимости для проекта на данный момент.
//  Done. Проверять, что блоки друг на друга не наезжают, в противном случае изменять их размер

//  Done with adjustments.Обработка страниц в отдельном потоке. Учитывать открытую страницу, чтобы изначально проводить обработку на ней (как вариант - создавать очередь, в которую будут записываться вперед
//  открытые страницы). При изменении размера страницы в первую очередь менять сам размер страниц, затем размер элементов на открытых, затем на остальных.
//  Пункт сделан с корректировками: необходимая коррекция производится только для отображаемых страниц.

//  Добавить поле ввода для перевода, которое будет отображать последнее переведенное слово.
//  Canceled. При наличии похожих символов, но из других языков, выводить сообщение об этом. В поле для последнего переведенного слова их не писать
//  Explanation. Так как есть критерии для лишних символов из латинского алфавита, но для ряда других нет, было решено это не реализовывать.
//  Done. Сделать в потенциально рискованных местах try catch
//  Done. Добавить логи работы программы и исключений
//  Canceled. Обработка данных должна быть в параллельном потоке, чтобы приложение быстро запускалось и подгружало все остальное
//  Explanation. Скорость вроде бы только замедлилась.

//  Done. Проверять те слова, на которые разбил анализатор с теми, которые выделил изначальный просмотр, чтобы определять их границы более верно. Но тогда придется обрабатывать ответ и искать схожие выражения. Кроме того, это не даст возможности смотреть слова отдельно. Поэтому лучше дать возможность объединяться слова в выражения по нажатию на ctrl.
//  Explanation. Вероятно нет смысла проверять слова, т.к. разбиваются по группам те же самые слова. Объединение по ctrl возможно.
//  Canceled. Добавить поле ввода номера страницы
//  Explanation. Было решено не добавлять связянность между двумя элементами интерфейса через ExternalInterface. Данная функция не является принципиальной для этой программы.
//  Explanation. Информация о номере страницы была добавлена, но работала неточно. Было решено ее пока тоже убрать.
//  Explanation. Видимо дело в том, что не учитывается расстояние между страницами по вертикали.

//  Done. Добавить выбор языков
//  Done. Сохранять настройки
//  Done. Реализовать открытие файла
//  Done. Убрать все локальные имена и пути
//  Done. Добавить настройку размера шрифта переводов
//  Done. Сделать загрузку файла сразу при его открытии, а не после скроллинга.
//  Done. _openedToolTipViewer = null убрать везде кроме места, в котором это происходит после закрытия подсказки.
//Сделать установщик.
//  Done. Поставить иконку.
//Протестировать.