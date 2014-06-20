using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeLibraryCSharp.Common;

namespace TicTacToeLibraryCSharp
{

    delegate void CellSelectedHandler(Cell sender);

    /// <summary>
    /// Represents a square on the game board.
    /// </summary>

    [Windows.UI.Xaml.Data.Bindable]
    [Windows.Foundation.Metadata.WebHostHidden]
    public sealed class Cell : Common.BindableBase
    {
        public Cell(int index, Windows.UI.Xaml.Media.Brush foregroundBrush, GameProcessor gameProcessor) 
            : this(index, foregroundBrush, gameProcessor, '\0')
        {}

        public Cell(int index, Windows.UI.Xaml.Media.Brush foregroundBrush, GameProcessor gameProcessor, char mark)
        {
            m_index = index;
            m_mark = mark;
            m_gameProcessor = new WeakReference(gameProcessor);

            ForegroundBrush = foregroundBrush;
            m_selectCommand = new DelegateCommand(new ExecuteDelegate(Select), null);

        }

        public string Text
        {
            get
            {
                return new string(new Char[] { m_mark });
            }
            
        }

        public System.Windows.Input.ICommand SelectCommand
        {
            get
            {
                return m_selectCommand;
            }
        }

        public Windows.UI.Xaml.Media.Brush ForegroundBrush
        {
            get
            {
                return m_foregroundBrush;
            }
            set
            {
                if (value != m_foregroundBrush)
                {
                    m_foregroundBrush = value;
                    OnPropertyChanged("ForegroundBrush");
                }
            }
        }

        internal event CellSelectedHandler CellSelected;

        internal int Index
        {
            get { return m_index; }
        }

        internal char Mark
        {
            get { return m_mark; }
        }

        public void Select(object parameter)
        {
            var gameProcessor = (GameProcessor) m_gameProcessor.Target;
            if ((m_mark == '\0' || m_mark == ' ') && gameProcessor != null && gameProcessor.IsUsersTurn())
            {
                m_mark = gameProcessor.CurrentPlayer.Symbol.Symbol;
                OnPropertyChanged("Text");

                CellSelected(this);
            }
        }

        WeakReference m_gameProcessor;
        //GameProcessor m_gameProcessor;

        int m_index;
        char m_mark;
        System.Windows.Input.ICommand m_selectCommand;
        Windows.UI.Xaml.Media.Brush m_foregroundBrush;

        public void Clear()
        {
            // User has selected another site.
            m_mark = ' ';
            OnPropertyChanged("Text");
        }
    }
}
