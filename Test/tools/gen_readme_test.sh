#!/usr/bin/env sh

output_bash() {
    local header="$1"
    local cmd="$2"

    echo "### $header"
    echo '```bash'
    echo "> $cmd"
    $cmd
    echo '```'
}

echo '## Example'
echo '```csharp'
cat ./Test.cs
echo '```'

output_bash 'Usage help'                'dotnet run'
output_bash 'Full help'                 'dotnet run -- --help'
output_bash 'Correct parsing'           'dotnet run -- positional -l 1,2,3,4 -o 4 -p80 -f'
output_bash 'Overriding parsing'        'dotnet run -- positional -e 1234 -o 4'
output_bash 'Post argument operands'    'dotnet run -- positional -e 1234 -o 4 -p80 -- some operands'
output_bash 'Conflicting parsing error' 'dotnet run -- positional -c 5 -o 4'
output_bash 'Validation error'          'dotnet run -- positional -p1234567'
output_bash 'Type conversion error'     'dotnet run -- positional -o "string"'
