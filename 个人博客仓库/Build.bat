cd %~dp0

hexo clean
hexo g
hexo d

REM 将 public 里面的静态站点数据全部 copy 到外层,GitHub 上面会展示出来的
copy -rvf public/* ../