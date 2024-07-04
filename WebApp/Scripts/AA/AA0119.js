Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "計量單位維護";

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '計量單位代碼',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load,
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiUnitcodeAA0119', { // 定義於/Scripts/app/store/MiUnitcodeAA0119.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AA0119/Create'; // AA0119Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0119/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0119/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });

    function setFormT1(x, t) {          //做畫面隱藏等控制
        viewport.down('#t1Grid').mask();
        viewport.down('#form').expand();
        viewport.down('#form').setTitle(t + T1Name);

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            T1Form.getForm().reset();
            var r = Ext.create('WEBAPP.model.MiUnitcode'); // /Scripts/app/model/MiUnitcode.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容;可問
            u = f.findField("UNIT_CODE"); // UNIT_CODE在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else{ //U or D
            u = f.findField('UI_CHANAME');
        }
        f.findField('x').setValue(x);  //T1Form設定為I or U or D
        f.findField('UI_CHANAME').setReadOnly(false);
        f.findField('UI_ENGNAME').setReadOnly(false);
        f.findField('UI_SNAME').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30
        },{
            text: "計量單位代碼",
            dataIndex: 'UNIT_CODE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "中文名稱",
            dataIndex: 'UI_CHANAME',
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "英文名稱",
            dataIndex: 'UI_ENGNAME', 
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "簡稱",
            dataIndex: 'UI_SNAME',
            style: 'text-align:left',
            align: 'left',
            width: 250
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1RecLength === 0);      //新增完後T1RecLength是0，無法修改
        T1Grid.down('#delete').setDisabled(T1RecLength === 0);      //選擇左側T1Grid，一個會有T1LastRec，則可以修改
        if (T1LastRec) {
            viewport.down('#form').expand();
            isNew = false;
            T1Form.loadRecord(T1LastRec); //資料從T1Grid to T1Form
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('UNIT_CODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T1Form.getForm().reset();  //右側資料清空
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
        { //控制項為了Update Insert Delete
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },{
            fieldLabel: '計量單位代碼',
            name: 'UNIT_CODE',
            enforceMaxLength: true,
            maxLength: 6,
            regexText: '只能輸入英文字母與數字',
            regex: /^[\w]{0,10}$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }, {
            fieldLabel: '中文名稱',
            name: 'UI_CHANAME',
            enforceMaxLength: true,
            maxLength: 60,
            readOnly: true,
            xtype: 'textareafield'
        }, {
            fieldLabel: '英文名稱',
            name: 'UI_ENGNAME',
            enforceMaxLength: true,
            maxLength: 60,
            readOnly: true,
            xtype: 'textareafield'
        }, {
            fieldLabel: '簡稱',
            name: 'UI_SNAME',
            enforceMaxLength: true,
            maxLength: 30,
            readOnly: true,
            xtype: 'textareafield'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('UI_CHANAME').getValue() == ''
                        && this.up('form').getForm().findField('UI_ENGNAME').getValue() == '')
                        Ext.Msg.alert('提醒', '中文名稱或英文名稱至少需輸入一種');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2); //EX:修改是否確定?
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        }
                        );
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,  //導到後端API
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) { //insert update 問
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            T1Query.getForm().findField('P0').setValue(f2.findField('UNIT_CODE').getValue());
                            T1Query.getForm().findField('P0').focus();
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask(); //左邊refresh結束
        viewport.down('#form').collapse();
        var f = T1Form.getForm();
        //f.reset(); //在 setFormT1a 有設定
        f.getFields().each(function (fc) { //右側欄位Read Only
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ,
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
    viewport.down('#form').collapse(); //收起來

});