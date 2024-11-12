Feature: GmailSendRecieve
	In order to communicate with other people via email
	Registered user should be able to send and receive 
	Emails from gmail website

@close_browser

Scenario Outline: Send and receive message
	Given I have navigated to gmail page
	And I see gmail page fully loaded
	When I type '<email>' address as
	And I press Next button
	And I type reg’s '<pass>'
	And I click on login button
	When I press Write message button
	And Type my '<email>' in field To
	And Type some '<text>' in field Subject
	And Click Send button
	And I navigate to Sended messages
	Then I should see new message with subject '<text>'
	And I navigate to Inbox messages
	And I should see new message with subject '<text>'
	Examples:
	| text | email | pass |
	|some text	| seleniumtest2000  | password2000 |
	
