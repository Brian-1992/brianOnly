Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var viewModel = Ext.create('WEBAPP.store.AB.AB0045VM');   // 定義於/Scripts/app/store/AB/AB0045VM.js

    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "扣庫時點維護檔";  
    var T1Rec = 0;
    var T1LastRec = null;

    var oldp0 = '';
    var newp0 = '';
    
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Store = viewModel.getStore('ME_CSTM');              // 定義於/Scripts/app/model/ME_CSTM.js 

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        viewport.down('#form').collapse();
        T1Store.getProxy().setExtraParam("p0", "");
        T1Store.load({
            params: {
                start: 0
            },
            callback: function (records, operation, success) {
                if (success) {
                    //Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                    //if (records.length == 0)
                    //    Ext.Msg.alert('沒有符合的資料!.......')
                    //else if (records.length > 0)
                    //    T1Tool.setDisabled(false);
                } else {
                    //Ext.getCmp('btnQry2').setDisabled(true);
                    //Ext.Msg.alert('訊息', 'failure!');
                }
            }
        })

    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AB0045/Create'; // AA0045Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0045/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0045/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            var r = Ext.create('WEBAPP.model.ME_CSTM'); // /Scripts/app/model/ME_CSTM
            T1Form.loadRecord(r);       // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("CSTM");  // 調劑類別在新增時才可填寫
            f.findField('CSTM').setReadOnly(false);        // 扣庫時點
            u.clearInvalid();
         }
        else {
            u = f.findField('CSTM');
            f.findField('CSTM').setReadOnly(false);        // 扣庫時點
        }

        f.findField('x').setValue(x);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    layout: 'fit',
            //    items: [T1Query]
            //    },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "扣庫時點",
            dataIndex: 'CSTM',
            width: 250 
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('CSTM');
            oldp0 = f.findField('CSTM').getValue();
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('CSTM').setReadOnly(true);
        }
        else {
            T1Form.getForm().reset();
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
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },
        {   xtype: 'textfield',
            fieldLabel: '扣庫時點',
            name: 'CSTM',
            id: 'CSTM_ID',
            enforceMaxLength: true,
            maxLength: 40,
            regexText: '只能輸入4位數字',
            regex: /^\d{4}$/,        // 用正規表示式限制只可輸入4個數字
            readOnly: true,
            fieldCls: 'required'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('CSTM').getValue() == '')
                        Ext.Msg.alert('提醒', '至少需輸入');
                    else {
                        var cstmtime = parseInt(this.up('form').getForm().findField('CSTM').getValue());
                        if (cstmtime < 0 || cstmtime > 2359) {
                            Ext.Msg.alert('提醒', '調劑時間至少需輸入0000-2359 之間!');
                            msglabel('訊息區:調劑時間至少需輸入 0000-2359 之間!');
                        } else {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            });
                        }
                    }
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
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
                url: T1Set,
                params:
                {
                    p0: oldp0,
                    newp0: Ext.getCmp('CSTM_ID').getValue(),
                },
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Store.load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            T1Store.load();
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
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        viewport.down('#form').collapse();
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
        },
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

    T1Load(); // 進入畫面時自動載入一次資料

});
