#
# Be sure to run `pod lib lint GuruAnalytics.podspec' to ensure this is a
# valid spec before submitting.
#
# Any lines starting with a # are optional, but their use is encouraged
# To learn more about a Podspec see https://guides.cocoapods.org/syntax/podspec.html
#

Pod::Spec.new do |s|
  s.name             = 'GuruAnalyticsLib'
  s.version          = '0.3.9'
  s.summary          = 'A short description of GuruAnalytics.'

# This description is used to generate tags and improve search results.
#   * Think: What does it do? Why did you write it? What is the focus?
#   * Try to keep it short, snappy and to the point.
#   * Write the description between the DESC delimiters below.
#   * Finally, don't worry about the indent, CocoaPods strips it!

  s.description      = <<-DESC
TODO: Add long description of the pod here.
                       DESC

  s.homepage         = 'https://github.com/castbox/GuruAnalytics_iOS'
  # s.screenshots     = 'www.example.com/screenshots_1', 'www.example.com/screenshots_2'
  s.license          = { :type => 'MIT', :file => 'LICENSE' }
  s.author           = { 'devSC' => 'xiaochong2154@163.com' }
  # s.source           = { :git => 'git@github.com:castbox/GuruAnalytics_iOS.git', :tag => s.version.to_s }
  s.source           = { :tag => s.version.to_s }
  # s.social_media_url = 'https://twitter.com/<TWITTER_USERNAME>'

  s.ios.deployment_target = '11.0'
  s.swift_version = '5'
  s.source_files = 'GuruAnalytics/Classes/**/*'
  # s.resource_bundles = {
  #   'GuruAnalytics' => ['GuruAnalytics/Assets/*.png']
  # }
    
  # s.public_header_files = 'Pod/Classes/**/*.h'
  # s.frameworks = 'UIKit', 'MapKit'
  # s.dependency 'AFNetworking', '~> 2.3'
  s.dependency 'RxCocoa', '~> 6.7.0'
  s.dependency 'Alamofire', '~> 5.9'
  s.dependency 'FMDB', '~> 2.0'
  s.dependency 'GzipSwift', '~> 5.0'
  s.dependency 'CryptoSwift', '~> 1.0'
  s.dependency 'SwiftyBeaver', '~> 1.0'
    
  s.subspec 'Privacy' do |ss|
      ss.resource_bundles = {
            s.name => 'GuruAnalytics/Assets/PrivacyInfo.xcprivacy'
      }
  end
end
