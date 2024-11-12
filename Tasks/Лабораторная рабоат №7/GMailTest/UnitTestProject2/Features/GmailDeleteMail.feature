Feature: GmailDeleteMail
	In order to clean my email from spam messages
	With "some text" subject, I need a checkbox and
	a "Delete" button



@do_some_messages
@close_browser

Scenario Outline: Delete messages

	Given I should see new message with subject '<text>'
	When I should select all messages with subject '<text>'
	And I should click delete button
	Then I navigate to Inbox messages
	And I should not see message with subject '<text>'
	Examples:
	| text | email | pass |
	|some text	| seleniumtest2000  | password2000 |
