using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FuckSunrun.Models.Dtos
{
    public class BasePaginationModel
    {
        [BindRequired]
        [Range(0,int.MaxValue)]
        public int Page { get; set; }

        [BindRequired]
        [Range(0, int.MaxValue)]
        public int Limit { get; set; }
    }
}

