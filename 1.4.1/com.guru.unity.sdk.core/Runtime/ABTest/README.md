
# ABTest 中台接口

- [相关实现技术文档链接](https://docs.google.com/document/d/1AG9PLq-dI0plIati2qgVpuD5QchNO2P8phY1GWr8SeQ/edit?pli=1#heading=h.906ruqltpzqz)


## 云控配置
- 需要根据每个测试来配置对应的属性和ID

  ```javascript
  // 参数 KEY 构成
  guru_ab_    +    231009   +     01
      ^               ^            ^
  固定前缀           年月日       实验序号 
  
  // 参数值构成
  "A" 或 "B"
  "C" 或 "D"
   
  ```
  

- 属性字段是追加在云控参数体内的
    ```json
    // 云控的json 参数数据体
    {
      "id": 1,
      "value": "test",
  
      "guru_ab_23100901": "A",   // 第一个实验的分组
      "guru_ab_23100902": "C"    // 第二个实验的分组
      
    }



    ```

## 结果验证

- 对于已经切换了自打点的项目, 需要BI组配合项目组, 抽取每组实验用户的数据, 可形成有效报告
- 项目接入后在启动时不会卡顿, 项目不会产生异常和崩溃




