﻿using System;
using NSaga;

namespace Samples
{
    public class InternalContainerSample
    {
        private ISagaMediator sagaMediator;
        private ISagaRepository sagaRepository;

        public void Run()
        {
            var builder = Wireup.UseInternalContainer()
                                .UseSqlServer() 
                                .WithConnectionStringName("NSagaDatabase")
                                .UseMessageSerialiser<JsonNetSerialiser>();

            sagaMediator = builder.ResolveMediator();

            sagaRepository = builder.ResolveRepository();

            var correlationId = Guid.NewGuid();

            StartSaga(correlationId);

            RequestVerificationCode(correlationId);

            ProvideVerificationCode(correlationId);

            CreateAccount(correlationId);

            var saga = sagaRepository.Find<AccountCreationSaga>(correlationId);
            var jamesName = saga.SagaData.Person.FullName;
            Console.WriteLine($"Taking information from SagaData; Person.FullName='{jamesName}'");
        }



        private void StartSaga(Guid correlationId)
        {
            var initialMessage = new PersonalDetailsVerification(correlationId)
            {
                DateOfBirth = new DateTime(1920, 11, 11),
                FirstName = "James",
                LastName = "Bond",
                HomePostcode = "MI6 HQ",
                PayrollNumber = "007",
            };

            var result = sagaMediator.Consume(initialMessage);
            if (!result.IsSuccessful)
            {
                Console.WriteLine(result.ToString());
            }
        }


        private void RequestVerificationCode(Guid correlationId)
        {
            var verificationRequest = new VerificationCodeRequest(correlationId);

            sagaMediator.Consume(verificationRequest);
        }



        private void ProvideVerificationCode(Guid correlationId)
        {
            var verificationCode = new VerificationCodeProvided(correlationId)
            {
                VerificationCode = "123456",
            };
            sagaMediator.Consume(verificationCode);
        }



        private void CreateAccount(Guid correlationId)
        {
            var accountCreation = new AccountDetailsProvided(correlationId)
            {
                Username = "James.Bond",
                Password = "James Is Awesome!",
                PasswordConfirmation = "james is awesome",
            };

            var excutionResult = sagaMediator.Consume(accountCreation);

            if (!excutionResult.IsSuccessful)
            {
                Console.WriteLine(excutionResult.ToString());
            }
        }
    }
}
