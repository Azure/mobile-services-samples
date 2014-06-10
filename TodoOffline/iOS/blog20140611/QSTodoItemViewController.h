//
//  QSTodoItemViewController.h
//  blog20140611
//
//  Created by Carlos Figueira on 6/9/14.
//  Copyright (c) 2014 MobileServices. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface QSTodoItemViewController : UIViewController <UITextFieldDelegate>

@property (nonatomic, weak) NSMutableDictionary *item;

@end
