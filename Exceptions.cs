using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace game_project
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message) : base(message){}
    }

    public class InvalidGameIDException : Exception{
        public InvalidGameIDException(string message) : base(message){}
    }

    public class InvalidMoveExceptionFilter : ExceptionFilterAttribute{
        public override void OnException(ExceptionContext context){
            if(context.Exception is InvalidMoveException){
                context.Result = new BadRequestObjectResult("Invalid input detected with message: "+context.Exception.Message+Environment.NewLine);
            }
        }
    }

     public class InvalidGameIDExceptionFilter : ExceptionFilterAttribute{
        public override void OnException(ExceptionContext context){
            if(context.Exception is InvalidGameIDException){
                context.Result = new BadRequestObjectResult("ID not found in database, with message: "+context.Exception.Message+Environment.NewLine);
            }
        }
    }
}