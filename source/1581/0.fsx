type Config = {
    home_folder: string option
    }

let default_config: Config = {
    home_folder: None}

let server_config = {
    default_config with
        home_folder = address
        }