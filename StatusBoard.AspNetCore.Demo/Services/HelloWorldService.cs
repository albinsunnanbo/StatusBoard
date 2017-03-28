using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatusBoard.AspNetCore.Demo.Services
{
    public class HelloWorldService
    {
        readonly HelloWorldProducerService producer;

        public HelloWorldService(HelloWorldProducerService producer)
        {
            this.producer = producer;
        }
        public string GetHelloText()
        {
            return producer.GetHelloText();
        }
    }
}
