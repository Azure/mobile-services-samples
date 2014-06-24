using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace TicTacToeMobileService.DataObjects
{
    public class GameInfo : EntityData
    {

        public string User1 { get; set; }

        public string User2 { get; set; }

        public string Board
        { get; set; }

        // The attribute is needed in order to prevent the field from
        // being omitted from the JSON (which causes problems on the client).
        [Required]
        public int GameResult { get; set; }

        // Same here.
        [Required]
        public int CurrentPlayerIndex
        { get; set; }

        public string User1Name
        { get; set; }

        public string User2Name
        { get; set; }
    }
}