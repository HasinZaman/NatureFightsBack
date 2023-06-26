use std::env;
use std::ffi::{CString, CStr};
use std::os::raw::c_char;
use std::ptr;

use openai::completions::Completion;
use openai::set_key;
use tokio::runtime::Runtime;

#[no_mangle]
pub extern "C" fn prompt_query_extern(prompt: *const c_char) -> *mut c_char {
    let input_string = if prompt.is_null() {
        return format_str("Error: WTF - this is a null pointer");
    } else {
        unsafe { CStr::from_ptr(prompt) }
            .to_string_lossy()
            .into_owned()
    };

    let response = process_prompt_query(&input_string);

    return format_str(&response);
}

pub fn process_prompt_query(prompt: &str) -> String {
    set_key(env::var("OPENAI_KEY").unwrap());

    let mut rt = Runtime::new().unwrap();
    let completion = rt.block_on(async move {
        Completion::builder("text-davinci-003")
            .prompt(prompt)
            .max_tokens(1024)
            .create()
            .await
    });

    match completion {
        Ok(completion) => {
            if let Some(response) = completion.choices.first() {
                response.text.clone()
            } else {
                eprintln!("Error: No response received");
                String::from("Error: No response received")
            }
        }
        Err(error) => {
            eprintln!("Error: {}", error);
            format!("Error: {}", error)
        }
    }
}

pub fn format_str(input: &str) -> *mut c_char {
    let c_string = match CString::new(input) {
        Ok(cstring) => cstring,
        Err(_) => return ptr::null_mut(),
    };

    c_string.into_raw()
}

#[no_mangle]
pub extern "C" fn free_string(ptr: *mut c_char) {
    if !ptr.is_null() {
        unsafe {
            CString::from_raw(ptr);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::ffi::CStr;
    use std::str;

    #[test]
    fn test_process_prompt_query() {
        let prompt = "Hello, world!";
        let response = process_prompt_query(prompt);
        println!("Response: {}", response);
        assert!(!response.is_empty(), "Empty response received");
    }

    #[test]
    fn test_prompt_query_extern() {
        let prompt = CString::new("Hello, world!").expect("CString::new failed");
        let response_ptr = prompt_query_extern(prompt.as_ptr());
        assert!(!response_ptr.is_null(), "Null response pointer");
        let response_cstr = unsafe { CStr::from_ptr(response_ptr) };
        let response_bytes = response_cstr.to_bytes();
        let response_str = str::from_utf8(response_bytes).expect("Invalid response string");
        let response = response_str.to_owned();
        println!("Response: {}", response);
        assert!(!response.is_empty(), "Empty response received");
        free_string(response_ptr);
    }
}