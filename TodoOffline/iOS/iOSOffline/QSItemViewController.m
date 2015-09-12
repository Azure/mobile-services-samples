// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "QSItemViewController.h"
#import "QSTodoService.h"

@interface QSItemViewController ()

@property (nonatomic, strong) IBOutlet UITextField *itemText;
@property (nonatomic, strong) IBOutlet UISegmentedControl *itemComplete;

@end

@implementation QSItemViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    UINavigationItem *nav = [self navigationItem];
    [nav setTitle:@"Todo Item"];
    
    NSDictionary *theItem = [self item];
    [self.itemText setText:[theItem objectForKey:@"text"]];
    
    BOOL isComplete = [[theItem objectForKey:@"complete"] boolValue];
    [self.itemComplete setSelectedSegmentIndex:(isComplete ? 0 : 1)];
    
    [self.itemComplete addTarget:self
                          action:@selector(completedValueChanged:)
                forControlEvents:UIControlEventValueChanged];
}

- (BOOL)textFieldShouldEndEditing:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (void)completedValueChanged:(id)sender {
    [[self view] endEditing:YES];
}

- (void)didMoveToParentViewController:(UIViewController *)parent
{
    if (![parent isEqual:self.parentViewController]) {
        NSNumber *completeValue = [NSNumber numberWithBool:self.itemComplete.selectedSegmentIndex == 0];
        
        Boolean changed =
            [self.item valueForKey:@"text"] != [self.itemText text] ||
            [self.item valueForKey:@"complete"] != completeValue;
        
        if (changed) {
            [self.item setValue:[self.itemText text] forKey:@"text"];
            [self.item setValue:completeValue forKey:@"complete"];
            
            self.editCompleteBlock(self.item);
        }
    }
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/

@end
