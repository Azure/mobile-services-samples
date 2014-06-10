//
//  QSTodoItemViewController.m
//  blog20140611
//
//  Created by Carlos Figueira on 6/9/14.
//  Copyright (c) 2014 MobileServices. All rights reserved.
//

#import "QSTodoItemViewController.h"

@interface QSTodoItemViewController ()

@property (nonatomic, strong) IBOutlet UITextField *itemText;
@property (nonatomic, strong) IBOutlet UISegmentedControl *itemComplete;

@end

@implementation QSTodoItemViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    
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

- (void)completedValueChanged:(id)sender {
    [[self view] endEditing:YES];
}

- (void)viewWillDisappear:(BOOL)animated {
    [self.item setValue:[self.itemText text] forKey:@"text"];
    [self.item setValue:[NSNumber numberWithBool:self.itemComplete.selectedSegmentIndex == 0] forKey:@"complete"];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (BOOL)textFieldShouldEndEditing:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

@end
