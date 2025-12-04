using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.Common
{
    public class BaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }

        public static BaseResponse<T> Success(T data)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Error = null
            };
        }

        public static BaseResponse<T> Fail(string error)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Error = error,
                Data = default
            };
        }
    }
}
