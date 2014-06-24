using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace TicTacToeMobileService.DataObjects
{
    public class Game : EntityData
    {
        public string User1 { get; set; }
        public string User2 { get; set; }
        public string Board { get; set; }
        public int GameResult { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public string Winner { get; set; }
    }
}