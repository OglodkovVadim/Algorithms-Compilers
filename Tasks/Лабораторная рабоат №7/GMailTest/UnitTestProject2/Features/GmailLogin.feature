Feature: Gmail loging
	In order to use google mail on website
	Registered user should be able to login to his account

Background: Log of
	Given gmail user is currently logged off

@close_browser

Scenario Outline: reg successfully loging
	Given I have navigated to gmail page
	And I see gmail page fully loaded
	When I type '<email>' address as
	And I press Next button
	And I type reg’s '<pass>'
	And I click on login button
	Then I should see reg’s mail
	Examples:
	| email | pass    |
	| seleniumtest2000  | password2000 |

Scenario: unreg can’t loging
	Given I have navigated to gmail page
	And I see gmail page fully loaded
	When I type unreg email address
	| email |
	| unreg | 
	And I press Next button
	Then I should see that unreg isn’t registered yet 

