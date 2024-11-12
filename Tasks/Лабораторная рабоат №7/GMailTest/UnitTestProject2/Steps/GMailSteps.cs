using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace Gmail
{
    [Binding]
    public class GMailSteps : Steps
    {
       
        IWebDriver driver = new ChromeDriver();
        //tried Baseclass.Contrib.SpecFlow.Selenium.NUnit class for dinamicaly use Chrome, Firefox, IE, but meet troubles on build stage


        ///////  For @do_some_messages  ///////
        string login = "seleniumtest2000";
        string pass = "password2000";
        string subjText = "some text"; 
        ///////////////////////////////////////

        public GMailSteps()
        {
            //driver.Manage().Timeouts().ImplicitWait(TimeSpan.FromSeconds(10));
            //driver.Manage().Timeouts().ImplicitWait = System.TimeSpan
            
        }

        [Given(@"gmail user is currently logged off")]
        public void GivenGmailUserIsCurrentlyLoggedOff()
        {
            driver.Navigate().GoToUrl("https://accounts.google.com/SignOutOptions?hl=ru&continue=https://mail.google.com/mail&service=mail");
            driver.FindElement(By.Name("signout")).Click();
        }
        
        [Given(@"I have navigated to gmail page")]
        public void GivenIHaveNavigatedToGmailPage()
        {
            driver.Navigate().GoToUrl("https://accounts.google.com/ServiceLogin?service=mail&continue=https://mail.google.com/mail/&hl=ru#identifier");
        }
        
        [Given(@"I see gmail page fully loaded")]
        public void GivenISeeGmailPageFullyLoaded()
        {
            Assert.AreEqual("Gmail", driver.Title, "Page not loaded");
        }
        
        [When(@"I type '(.*)' address as")]
        public void WhenITypeRegAddressAs(string mailaddr)
        {
            driver.FindElement(By.Name("Email")).SendKeys(mailaddr);
        }
        
        [When(@"I press Next button")]
        public void WhenIPressNextButton()
        {
            driver.FindElement(By.Id("next")).Click();
        }
        
        [When(@"I type reg’s '(.*)'")]
        public void WhenITypeRegSPass(string s)
        {
            driver.FindElement(By.Name("Passwd")).SendKeys(s);
        }
        
        [When(@"I click on login button")]
        public void WhenIClickOnLoginButton()
        {
            driver.FindElement(By.Id("signIn")).Click();
        }
        
        [Then(@"I should see reg’s mail")]
        public void ThenIShouldSeeRegSMail()
        {
            System.Threading.Thread.Sleep(6000);
            Assert.AreEqual(driver.Title.StartsWith("Входящие"),true, "Main page not found or timeout expired");
        }
        
        [When(@"I type unreg email address")]
        public void WhenITypeUnregEmailAddress(Table table)
        {
            string login = table.Rows[0]["email"].ToString();
        }
        
        [Then(@"I should see that unreg isn’t registered yet")]
        public void ThenIShouldSeeThatUnregIsnTRegisteredYet()
        {
            bool exist = driver.FindElement(By.XPath("//span[contains(text(),'Введите адрес электронной почты.')]")).Displayed;
            Assert.AreEqual(exist, true, "You are logged on with unregistered gmail adress");
        }

        [When(@"I press Write message button")]
        public void WhenIPressWriteMessageButton()
        {
            driver.FindElement(By.XPath("//div[contains(@role,'button') and contains(.,'НАПИСАТЬ')]")).Click();
        }

        [When(@"Type my '(.*)' in field To")]
        public void WhenTypeMyInFieldTo(string emailaddr)
        {
            driver.FindElement(By.XPath("//textarea[contains(@name,'to')]")).SendKeys(emailaddr+"@gmail.com");
            driver.FindElement(By.XPath("//textarea[contains(@name,'to')]")).SendKeys(Keys.Return);
        }

        [When(@"Type some '(.*)' in field Subject")]
        public void WhenTypeSomeInFieldSubject(string somesubj)
        {
            driver.FindElement(By.XPath("//div[contains(@class,'aoD az6')]")).Click();
            driver.FindElement(By.XPath("//input[contains(@name,'subjectbox')]")).SendKeys(somesubj);
        }

        [When(@"Click Send button")]
        public void WhenClickSendButton()
        {
            driver.FindElement(By.XPath("//div[contains(@role,'button') and contains(.,'Отправить')]")).Click();
        }

        [When(@"I navigate to Sended messages")]
        public void WhenINavigateToSendedMessages()
        {
            driver.FindElement(By.PartialLinkText("Отправленные")).Click();
        }

        [Then(@"I navigate to Inbox messages")]
        public void ThenINavigateToInboxMessages()
        {
            driver.FindElement(By.PartialLinkText("Входящие")).Click();
        }

        [Then(@"I should not see message with subject '(.*)'")]
        public void ThenIShouldNotSeeMessageWithSubject(string subjText)
        {
            int count = driver.FindElements(By.XPath("//tr[contains(@class,'zA yO') or contains(@class,'zA zE')][./td/div/div/div/span[contains(.,'" + subjText + "')]]")).Count;
            Assert.AreEqual(0,count, "Not all messages with subject text <" + subjText + "> were deleted");
        }

        [Then(@"I should see new message with subject '(.*)'")]
        [Given(@"I should see new message with subject '(.*)'")]
        public void GivenIShouldSeeNewMessageWithSubject(string subjText)
        {
            string searchString = string.Format("//span[contains(.,'{0}') or contains(./b,'{0}')]", subjText);
            Assert.AreNotEqual(0, driver.FindElements(By.XPath(searchString)).Count,
                string.Format("Can't find message with subject <{0}>", subjText));
        }

        [When(@"I should select all messages with subject '(.*)'")]
        public void WhenIShouldSelectAllMessagesWithSubject(string subjText)
        {
            var element = driver.FindElements(By.XPath("//tr[contains(@class,'zA yO') or contains(@class,'zA zE')][./td/div/div/div/span[contains(.,'" + subjText + "')]]//div[@role='checkbox']"));
            foreach (var chbox in element)
            {
                chbox.FindElement(By.ClassName("T-Jo-auh")).Click();
            }
        }

        [When(@"I should click delete button")]
        public void WhenIShouldClickDeleteButton()
        {
            driver.FindElement(By.XPath("html/body/div[7]/div[3]/div/div[2]/div[1]/div[2]/div/div/div/div[1]/div/div[1]/div[1]/div/div/div[2]/div[3]")).Click();
        }

        [BeforeScenario("do_some_messages")]
        public void hook()
        {
            
            Given(string.Format("I have navigated to gmail page"));
            Given(string.Format("I see gmail page fully loaded"));
            When(string.Format("I type '{0}' address as", login));
            When(string.Format("I press Next button"));
            When(string.Format("I type reg’s '{0}'", pass));
            When(string.Format("I click on login button"));
            

            for (int i = 1; i <= 10; i++)
            {
                When(string.Format("I press Write message button"));
                When(string.Format("Type my '{0}' in field To", login));
                When(string.Format("Type some '{0}' in field Subject", subjText));
                When(string.Format("Click Send button"));
                System.Threading.Thread.Sleep(1000);
            }
            System.Threading.Thread.Sleep(20000);
        }
        [AfterScenario("close_browser")]
        public void CloseBrowser()
        {
            driver.Quit();
        }

    }
}
