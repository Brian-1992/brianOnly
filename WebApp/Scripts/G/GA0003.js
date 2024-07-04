Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BE0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = '';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var mLabelWidth = 70;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '電腦編號',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13, // 可輸入最大長度為100
                    padding: '0 4 4 4',
                    width: 200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '藥商名稱',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 4 4',
                    width: 300
                }

            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'textfield',
                fieldLabel: '藥品名稱',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 125, // 可輸入最大長度為100
                padding: '0 18 0 4',
                width: 400
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                    T1Load();
                    msglabel('訊息區:');
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.TC_MMAGEN', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
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
                    T1Set = '/api/GA0003/Create'; // BE0002Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/GA0003/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/GA0003/Delete';
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
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        f.findField("PUR_UNIT").setReadOnly(false);
        f.findField("IN_PURPRICE").setReadOnly(false);
        f.findField("PUR_SEQ").setReadOnly(false);


        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.TC_MMAGEN'); // /Scripts/app/model/PhVender.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField("MMCODE").setReadOnly(false);
            f.findField("AGEN_NAMEC").setReadOnly(false);

            f.findField("MMCODE").clearInvalid();
            f.findField("AGEN_NAMEC").clearInvalid();
            f.findField("PUR_UNIT").clearInvalid();
            f.findField("IN_PURPRICE").clearInvalid();

            f.findField("MMNAME_C").hide();
            f.findField("PUR_SEQ").setValue(1);

        }
        if (x == "U") {
            f.findField("MMCODE").hide();
            f.findField("AGEN_NAMEC").hide();
            f.findField("MMCODE_D").show();
            f.findField("AGEN_NAMEC_D").show();

        }

        f.findField('x').setValue(x);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

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
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
                        width: 40

        }, {
            text: "電腦編號",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 200
        }, {
            text: "單位劑量",
            dataIndex: 'PUR_UNIT',
            width: 80
        }, {
            text: "進貨單價",
            dataIndex: 'IN_PURPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
                text: "採購順序",
            dataIndex: 'PUR_SEQ',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {            
            xtype: 'hidden',
            dataIndex: 'CREATE_TIME'
        }, {
            xtype: 'hidden',
            dataIndex: 'CREATE_USER'
        }, {
            xtype: 'hidden',
            dataIndex: 'UPDATE_TIME'
        }, {
            xtype: 'hidden',
            dataIndex: 'UPDATE_USER'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField("MMCODE_D").setValue(f.findField("MMCODE").getValue());
            f.findField("AGEN_NAMEC_D").setValue(f.findField("AGEN_NAMEC").getValue());
            f.findField('x').setValue('U');

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
        }, {
            fieldLabel: '電腦編號',
            name: 'MMCODE',
            enforceMaxLength: true,
            maxLength: 13,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '電腦編號',
            name: 'MMCODE_D',
            hidden: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '藥品名稱',
            name: 'MMNAME_C',
        }, {
            xtype: 'textarea',
            fieldLabel: '藥商名稱',
            name: 'AGEN_NAMEC',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'

        }, {
            xtype: 'displayfield',
            fieldLabel: '藥商名稱',
            name: 'AGEN_NAMEC_D',
            hidden: true
        }, {
            fieldLabel: '單位劑量',
            name: 'PUR_UNIT',
            enforceMaxLength: true,
            maxLength: 6,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
        }, {
            xtype: 'numberfield',
            fieldLabel: '進貨單價',
            name: 'IN_PURPRICE',
            enforceMaxLength: true,
            minValue: 0,
            hideTrigger: true,
            allowDecimals: true,
            forcePrecision: true,
            maxLength: 11,
            maxValue: 99999999.99,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
            decimalPrecision: 2
        }, {
                xtype: 'textfield',
            fieldLabel: '採購順序',
            name: 'PUR_SEQ',
            enforceMaxLength: true,
            minValue: 1,
            maxLength: 5,
            maxValue: 99999,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
            maskRe: /[0-9]/,
            regex: /^([1-9][0-9]*|0)$/,
            regexText: '只能輸入數字',

        }, {
            xtype: 'displayfield',
            fieldLabel: '建立時間',
            name: 'CREATE_TIME',
            renderer: function (value) {
                if (value == '' || value == null)
                { return '';}
                var time = new Date(value);
                return Ext.util.Format.date(value, 'Xmd') + " " + time.toLocaleTimeString();
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '建立人員代碼',
            name: 'CREATE_USER'
        }, {
            xtype: 'displayfield',
            fieldLabel: '異動時間',
            name: 'UPDATE_TIME',
            renderer: function (value) {
                if (value == '' || value == null)
                { return ''; }
                var time = new Date(value);
                return Ext.util.Format.date(value, 'Xmd') + " " + time.toLocaleTimeString();
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '異動人員代碼',
            name: 'UPDATE_USER'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

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
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.MMCODE);
                            T1Query.getForm().findField('P1').setValue(v.AGEN_NAMEC);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
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
        f.findField("MMCODE").show();
        f.findField("AGEN_NAMEC").show();
        f.findField("MMNAME_C").show();

        f.findField("MMCODE_D").hide();
        f.findField("AGEN_NAMEC_D").hide();

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
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
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    

    //T1Form.getForm().findField('AGEN_NAMEC').inputEl.setMinHeight(21);
});




