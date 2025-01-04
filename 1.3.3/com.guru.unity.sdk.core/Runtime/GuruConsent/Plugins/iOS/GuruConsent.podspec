#
# Be sure to run `pod lib lint CastboxNetwork.podspec' to ensure this is a
# valid spec before submitting.
#
# Any lines starting with a # are optional, but their use is encouraged
# To learn more about a Podspec see http://guides.cocoapods.org/syntax/podspec.html
#

Pod::Spec.new do |s|
  s.name             = 'GuruConsent'
  s.version          = '1.4.6'
  s.summary          = 'Google GDPR'
  s.description      = 'Google GDPR'

  s.homepage         = 'https://github.com/castbox/GuruConsent-iOS'
  s.license          = { :type => 'MIT', :file => 'LICENSE' }
  s.author           = { 'LEE' => 'xiang.li@castbox.fm' }
  # s.source           = { :git => 'git@github.com:castbox/GuruConsent-iOS.git', :tag => s.version }
  s.source           = { :tag => s.version }

  s.frameworks = 'UIKit', 'AppTrackingTransparency'
  s.swift_version = '5.0'
  s.ios.deployment_target = '12.0'

  s.source_files  = "GuruConsent/Sources/**/*.swift"

  s.requires_arc = true
  
  s.static_framework = true
  
  s.default_subspec = 'Privacy'
  
  s.dependency 'GoogleUserMessagingPlatform', '2.3.0'
  
  s.subspec 'Privacy' do |ss|
      ss.resource_bundles = {
        s.name => 'GuruConsent/Resources/PrivacyInfo.xcprivacy'
      }
  end
end
