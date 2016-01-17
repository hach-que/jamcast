# JamCast

JamCast allows you to stream developer's desktops to projectors internally in a game jam.  It is being used in the Melbourne Global Game Jam 16.

**This software is experimental**.  It's still under a lot of development, and you should expect things to be rough, incomplete, or in some cases, just completely broken.

This repository is a mirror of https://code.redpointgames.com.au/diffusion/JC/ and we do not accept pull requests here.  To contribute to this repository, please contact [hachque](https://twitter.com/hachque) and we'll sort something out (right now there's only two developers and we have little time before the 2016 jam, so we might not be able to sort something out before then).

## Configuration & Building

### Configuration

There's a lot of rough edges here, and set up is quite complex.  We expect by next year (2017) this process will have simplified greatly.

You'll need a few things before you can use JamCast.  Most notably, you'll need a [Google Cloud account](https://cloud.google.com/) and a Google Cloud project with Pub/Sub and Cloud Storage enabled.  This is used by JamCast so that the bootstrap and client executables (running on developer machines) can talk to the controller and projector software.

You will also need a web server that can provide a signed OAuth token to access Google Cloud services.  Because Google Cloud service accounts can create tokens in any scope, you need a web server to perform the creation of the OAuth token, limiting it's use to the Pub/Sub access for clients, and Pub/Sub + Storage access for projectors.  This is quite complex, because you'll need web server with custom PHP code to generate the OAuth token.  We provide an example later in this README, but it is quite specific to our setup, and we unfortunately don't have time to simplify this setup before the 2016 Global Game Jam.

1. Create a Google Cloud project and save the project ID.
2. Create a Google Cloud service account from the "API Manager" section, under the "Credentials" subsection.  You need to create a "Service Account", not an "OAuth 2.0 Client ID".  When being asked to generate the key, save it in the recommended JSON format.
3. Turn on the "Cloud Storage API" and "Cloud Pub/Sub API" from the "Overview" page.  You may need to click on "More" to see these APIs.
4. Create a Google Cloud storage bucket with the same name as the project ID.
5. Configure a website using the PHP (or other language libraries) on your web server.  See the bottom of these README for an example.

### Building

Once you have completed the configuration prerequisites, run the `Protobuild.exe` executable in the repository to generate the solution files.  You will then have `JamCast.Windows.sln` which you can then build in Visual Studio.  We recommend Visual Studio Community 2015 as that's what it's been developed in.

We currently do not provide binaries because this software is under constant development.

## Usage at a jam

Once you've built the software, run the "Controller" application.  This is the only application you need to actually run after this point, so you can just copy the output of this application's build onto whatever computer you want to be the controller for the jam.  You will only run this software on one computer.

Upon starting the application, right-click in the panel on the left and select "New Jam".  Optionally rename it, and then double-click on it to edit the jam configuration if it's not already shown.

Enter the Google Cloud configuration and OAuth token endpoint URLs into the jam configuration, and then click "Generate Bootstrap".  This will ask you to save an executable file.  You should provide this file to all of the developers in the jam, or if you are hosting the jam in a location where you are providing computers, you should configure the bootstrap to run at startup (e.g. place it in the Startup menu folder).

When the bootstrap runs on a computer, you should see the computer's hostname appear underneath the jam in the controller.  By right-clicking on the computer, you can designate it as a client (the screen is broadcast) or a projector (the full screen projector application will run).

## Broadcasting to Twitch

To broadcast the jam to Twitch, run the bootstrap on an unused computer and designate it as a projector.  This will show a fullscreen window.  Normally you would run this on computers that are connected to actual projectors (we do this at the Melbourne Global Game Jam which is quite large, but also seperated into seperate rooms, so it gives developers good visibility on what other people are working on).

Once the projector is running, run some form of broadcasting software like [https://obsproject.com/](Open Broadcaster Software) to broadcast the projector's entire screen to Twitch.

## PHP OAuth authorization example

This is the PHP code we use on our server to generate an authorization token to interact with our jam.  You will almost certainly need to adapt it to your own server and to your own keys, as you won't be building on the same software framework as us:

```php
<?php
    $scopes = array(
      // Only provide the pub/sub authentication scope and the Cloud Storage
      // datascope, which means this credential can't access Datastore, etc.
      "https://www.googleapis.com/auth/pubsub",
    );
    if (idx($_GET, 'controller_secret') === '<SET AS THE STORAGE SECRET FOR THE CONTROLLER>') {
      // Give controllers the ability to write into Google Cloud Storage.
      $scopes[] = 
        "https://www.googleapis.com/auth/devstorage.read_write";
    }
  
    $client = new Google_Client();
    $client->setApplicationName("SET AS AN APPLICATION NAME (e.g. jamcast)");
    $client->setAssertionCredentials(new Google_Auth_AssertionCredentials(
      "SET AS THE EMAIL ADDRESS PROVIDED IN THE GOOGLE CLOUD SERVICE JSON FILE",
      $scopes,
      "SET AS THE FULL RSA PRIVATE KEY PROVIDED IN THE GOOGLE CLOUD SERVICE JSON FILE",
      null));
    $client->setDeveloperKey(
      "SET AS THE DEVELOPER KEY PROVIDED IN THE GOOGLE CLOUD SERVICE JSON FILE";
    
    if ($client->getAuth()->isAccessTokenExpired()) {
      $client->getAuth()->refreshTokenWithAssertion();
    }
    
    echo json_decode($client->getAuth()->getAccessToken());
?>
```
