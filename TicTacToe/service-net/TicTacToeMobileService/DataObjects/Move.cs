using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace TicTacToeMobileService.DataObjects
{
    public class Move : EntityData
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string NewBoard { get; set; }
        public int GameResult { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public string Winner { get; set; }
    }
}