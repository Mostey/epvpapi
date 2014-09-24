Introduction
===
This library is currently a work-in-progress and almost reached alpha status, please let us know if something does not work as expected or if you find a documentation article is lacking information.


References
===
These pages provide explanations and samples on how to properly use the library.

* [Sessions and Library extensions](https://github.com/Mostey/epvpapi/wiki/Sessions) 
* [Shoutbox](https://github.com/Mostey/epvpapi/wiki/Shoutbox)
* [Private Messages](https://github.com/Mostey/epvpapi/wiki/Private-Messages)
* [Blogs](https://github.com/Mostey/epvpapi/wiki/Blogs)
* [Sections](https://github.com/Mostey/epvpapi/wiki/Sections)
* [Users and Ranks](https://github.com/Mostey/epvpapi/wiki/Users-and-Ranks)
* [Profiles (Session Users)](https://github.com/Mostey/epvpapi/wiki/Profiles-(Session-Users))
* [Threads](https://github.com/Mostey/epvpapi/wiki/Threads)
* [The Black Market (Treasures, Transactions, ...)](https://github.com/Mostey/epvpapi/wiki/The-Black-Market-(Treasures,-Transactions,-...))


Samples and Troubleshooting
===
If something does not work correctly, please refer to our unit tests first which are located in the `/test/UnitTests` directory of the repository. We aim to update them simultaneously as the library grows so they also act as samples you may use.

In case you want to run the tests by yourself, create the text file `credentials.txt` in your `%LocalAppData%/epvpapi` folder and fill in your credentials in the following format:

   `<UserName>:<UserID>:<MD5PasswordHash>:<TBMAPISecretWord>`
   
   **Example:** `Mostey:99999:FFFFFFFFFFFFFFFFFFFFFFFFFFF:mysecretword`
   

License
===
  
    The MIT License (MIT)
    
    Copyright (c) 2014 
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
