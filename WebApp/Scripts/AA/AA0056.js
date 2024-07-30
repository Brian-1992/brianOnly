Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0056/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "藥品小單位轉原包裝出帳消耗維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0056M', { // 定義於/Scripts/app/store/AA/AA0056M.js
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: '1'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/AA0056/UpdateM';
                    setFormT1("U", '修改');
                }
            }]
    });

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '10',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 80,
            width: 260
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '母藥院內碼', //白底顯示
                name: 'MMCODE',
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '藥品名稱', //白底顯示
                name: 'MMNAME_E',
                readOnly: true,
                width: '100%'
            }, {
                xtype: "numberfield",
                fieldLabel: '母藥轉換量',
                name: 'E_SONTRANSQTY',
                enforceMaxLength: true,
                maxLength: 15,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                hideTrigger: true,
                blankText: "請輸入母藥轉換量",
                minValue: 1,
                minText: "母藥轉換量需大於0",
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '母藥註記', //白底顯示
                name: 'E_PARCODE_CODE',
                readOnly: true,
                submitValue: true
            }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if ((T1Form.getForm().findField("E_SONTRANSQTY").getValue()) == null) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>母藥轉換量不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>母藥轉換量不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("E_SONTRANSQTY").getValue()) < 1) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>母藥轉換量需大於0</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>母藥轉換量需大於0</span>，請重新輸入。");
                }
                else {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel(" 輸入資料格式有誤");
                    }
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "母藥院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            width: 300
        }, {
            text: "母藥轉換量",
            dataIndex: 'E_SONTRANSQTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "母藥註記",
            dataIndex: 'E_PARCODE_NAME',
            width: 70
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                if (T1LastRec) {
                    viewport.down('#form').expand();
                    msglabel("");
                }
            }
        }
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        T1Form.setVisible(true);
        T2Form.setVisible(false);

        var f = T1Form.getForm();
        msglabel("");

        u = f.findField('E_SONTRANSQTY');
        f.findField('x').setValue(x);
        f.findField('E_SONTRANSQTY').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
    }

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    msglabel(" 資料修改成功");
                    myMask.hide();
                    viewport.down('#form').setCollapsed("true");
                    T1LastRec = false;
                    T1Form.getForm().reset();
                    T1Load();
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            msglabel(" Form fields may not be submitted with invalid values");
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            msglabel(" Ajax communication failed");
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            msglabel(" " + action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    //點選master的項目後
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
        }
        else {
            T1Form.getForm().reset();
        }
        T2Load();
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();

        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.readOnly = true;
            fc.setReadOnly(true);
        });

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        if (!T1LastRec) {
            viewport.down('#form').setCollapsed("true");
        }
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var T2Store = Ext.create('WEBAPP.store.AA.AA0056D', { // 定義於/Scripts/app/store/AA/AA0056D.js
        listeners: {
            //載入前將master的院內碼關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("MMCODE").getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/AA0056/UpdateD';
                    setFormT2("U", '修改');
                }
            }
        ]
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '10',
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
        }, {
            xtype: 'displayfield',
            fieldLabel: '子藥院內碼', //白底顯示
            name: 'MMCODE',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '藥品名稱', //白底顯示
            name: 'MMNAME_E',
            readOnly: true
        }, {
            xtype: "numberfield",
            fieldLabel: '子藥轉換量',
            name: 'E_SONTRANSQTY',
            enforceMaxLength: true,
            maxLength: 15,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            hideTrigger: true,
            minValue: 1,
            blankText: "請輸入子藥轉換量",
            minText: "子藥轉換量需大於0",
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '子藥註記', //白底顯示
            name: 'E_PARCODE_CODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '母藥院內碼', //白底顯示
            name: 'E_PARORDCODE',
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("E_SONTRANSQTY").getValue()) == null) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>子藥轉換量不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>子藥轉換量不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("E_SONTRANSQTY").getValue()) < 1) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>子藥轉換量需大於0</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>子藥轉換量需大於0</span>，請重新輸入。");
                }
                else {
                    if (this.up('form').getForm().isValid()) { // 檢查T2Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel(" 輸入資料格式有誤");
                    }
                }

            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                msglabel("");
            }
        }]
    });

    //按了新增/刪除/修改後
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        T1Form.setVisible(false);
        T2Form.setVisible(true);
        var f2 = T2Form.getForm();
        msglabel("");

        u = f2.findField('E_SONTRANSQTY');
        f2.findField('x').setValue(x);
        f2.findField('E_SONTRANSQTY').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "子藥院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            width: 300,
            sortable: true
        }, {
            text: "子藥轉換量",
            dataIndex: 'E_SONTRANSQTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "子藥註記",
            dataIndex: 'E_PARCODE_NAME',
            width: 70,
            sortable: true
        }, {
            text: "母藥院內碼",
            dataIndex: 'E_PARORDCODE',
            width: 80,
            sortable: true
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                if (T2LastRec) {
                    viewport.down('#form').expand();
                    msglabel("");
                }
            }
        }
    });

    //點選detail項目後
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();

            f.findField('x').setValue('U');
            T2Grid.down('#edit').setDisabled(false);
        }
        else {
            T2Form.getForm().reset();
            T2Grid.down('#edit').setDisabled(true);
        }
    }

    //Detail按儲存之後
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    msglabel(" 資料修改成功");
                    viewport.down('#form').setCollapsed("true");
                    T2Cleanup();
                    T2Load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            msglabel(" Form fields may not be submitted with invalid values");
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            msglabel(" Ajax communication failed");
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            msglabel(" " + action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.readOnly = true;
            fc.setReadOnly(true);
        });
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        if (!T2LastRec) {
            viewport.down('#form').setCollapsed("true");
        }
        setFormT2a();
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }
        ]
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    function T2Load() {
        try {
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }

    T1Load();
});
