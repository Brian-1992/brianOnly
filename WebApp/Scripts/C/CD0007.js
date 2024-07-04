Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var viewModel = Ext.create('WEBAPP.store.CD0007VM');   // 定義於/Scripts/app/store/CD0007VM.js
    var WHStore = viewModel.getStore('CD0007WH_NO');
    var T1Store = viewModel.getStore('CD0007PICK');        // 定義於/Scripts/app/model/CD0007PICK.js

    //var WhnoComboGet = '/api/CD0002/GetWhnoCombo';
    var WhnoComboGet = '../../../api/CD0007/GetWH_NoCombo';

    var T1Set = ''; // 新增/修改/刪除
    var T1Get = '../../../api/CD0007/QueryD';      // ../../../api/CD0007/QueryD
    var T1Name = "揀貨統計表";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var sortMethod = Ext.create('Ext.form.RadioGroup', {
        fieldLabel: '排序方式',
        id: 'radioSortMethod',
        vertical: false,
        column: 4,
        items: [
            { name: 'IS_SORT_NM', boxLabel: '總單號數', inputValue: '1', checked: true },
            { name: 'IS_SORT_NM', boxLabel: '總品項數', inputValue: '2' },
            { name: 'IS_SORT_NM', boxLabel: '總件數', inputValue: '3' },
            { name: 'IS_SORT_NM', boxLabel: '差異件數', inputValue: '4' }
        ]
    });

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME', 'WH_KIND', 'WH_USERID'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: WhnoComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 85;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,     // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                    xtype: 'combo',
                    store: whnoQueryStore,
                    name: 'WH_ID_CODE',
                    id: 'WH_ID_CODE',
                    width: 263,
                    fieldLabel: '庫房代碼',
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    queryMode: 'local',
                    fieldCls: 'required',
                    anyMatch: true,
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>'
                }
                ,
                {
                    xtype: 'displayfield',
                    name: 'WH_NAME',
                    id: 'WH_NAME',
                    labelWidth: 60,
                    store: WHStore,
                    value: '',
                    width: 150,
                    readOnly: true,
                    padding: '2 4 0 6'
                }
                ,
                {
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
                        Ext.getCmp("WH_ID_CODE").setValue('');     // 庫房代碼 clear
                        Ext.getCmp('WH_NAME').setValue('');
                    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '揀貨日期',
                    width: 170,
                    vtype: 'dateRange',
                    dateRange: { end: 'P2' },
                    padding: '0 4 0 0'
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '至',
                    width: 90,
                    labelWidth: 10,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P1' },
                    padding: '0 4 0 0'
                }

            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            items: [
                sortMethod
            ]
        }
        ]
    });

    function T1Load() {
        if (!T1Query.getForm().findField('WH_ID_CODE').getValue()) {
            Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
        } else {
            T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('WH_ID_CODE').getValue());
            T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
            T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
            T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('IS_SORT_NM').getGroupValue());
            T1Store.load({
                params: {
                    start: 0
                }
                ,
                callback: function (records, operation, success) {
                    if (success) {
                        //Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                        if (records.length == 0) {
                            //Ext.Msg.alert('沒有符合的資料!')
                        } else {
                            //    if (records.length > 0) {
                            //        T1Tool.moveFirst();
                            //        T1Tool.setDisabled(false);
                            //    }
                        }
                    } else {
                        // Ext.getCmp('btnQry2').setDisabled(true);
                        Ext.Msg.alert('訊息', 'failure!');
                    }
                }
            })
        }
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '新增', handler: function () {
            //        T1Set = '/api/CD0007/Create'; // BE0002Controller的Create
            //        msglabel('訊息區:');
            //        setFormT1('I', '新增');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/CD0007/Update';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/BE0002/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhVender'); // /Scripts/app/model/PhVender.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
        }
        else {
            u = f.findField('AGEN_NAMEC');
        }
        f.findField('x').setValue(x);
        f.findField('AGEN_NAMEC').setReadOnly(false);
        f.findField('AGEN_NAMEE').setReadOnly(false);
        f.findField('AGEN_ADD').setReadOnly(false);
        f.findField('AGEN_FAX').setReadOnly(false);
        f.findField('AGEN_TEL').setReadOnly(false);
        f.findField('AGEN_ACC').setReadOnly(false);
        f.findField('UNI_NO').setReadOnly(false);
        f.findField('AGEN_BOSS').setReadOnly(false);
        f.findField('REC_STATUS').setReadOnly(false);
        f.findField('EMAIL').setReadOnly(false);
        f.findField('EMAIL_1').setReadOnly(false);
        f.findField('AGEN_BANK').setReadOnly(false);
        f.findField('AGEN_SUB').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        /////////u.focus();
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
            xtype: 'rownumberer'
        }, {
            text: "揀貨人員",
            dataIndex: 'PICK_USERNAME',
            renderer: function (value) {
                switch (value) {
                    case '總計': return '<span style="color:black; font-weight:bold">總計</span>'; break;
                    default: return value; break;
                }
            },
            width: 130
        }, {
            text: "總單號數",
            dataIndex: 'DOCNO_CNT',
            width: 130
        }, {
            text: "總品項數",
            dataIndex: 'ITEM_SUM',
            width: 130
        }, {
            text: "總件數",
            dataIndex: 'PICK_QTY_SUM',
            width: 150
        }, {
            text: "差異件數",
            dataIndex: 'DIFFQTY_SUM',
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                //alert("before T1LastRec:" + T1LastRec);
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        //T1Grid.down('#edit').setDisabled(T1Rec === 0);
        // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        //alert("T1LastRec:" + T1LastRec);
        if (T1LastRec) {
            isNew = false;
            //alert("T1LastRecx:" + T1LastRec);
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('PICK_USERNAME');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            //if (T1LastRec.data['REC_STATUS'] == 'X')
            //    T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
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
            fieldLabel: '揀貨人員',
            name: 'PICK_USERNAME',
            enforceMaxLength: true,
            //maxLength: 6,
            //regexText: '只能輸入英文字母與數字',
            //regex: /^[\w]{0,6}$/, // 用正規表示式限制可輸入內容
            readOnly: true //,
            //allowBlank: false, // 欄位為必填
            //fieldCls: 'required'
        }, {
            fieldLabel: '總單號數',
            name: 'DOCNO_CNT',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '總品項數',
            name: 'ITEM_SUM',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '總件數',
            name: 'PICK_QTY_SUM',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '差異件數',
            name: 'DIFFQTY_SUM',
            enforceMaxLength: true,
            maxLength: 14,
            readOnly: true
        }
        ]
        //,
        //buttons: [{
        //    itemId: 'submit', text: '儲存', hidden: true,
        //    handler: function () {
        //        if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
        //            if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
        //                && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
        //                Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
        //            else {
        //                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
        //                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
        //                    if (btn === 'yes') {
        //                        T1Submit();
        //                    }
        //                }
        //                );
        //            }
        //        }
        //        else {
        //            Ext.Msg.alert('提醒', '輸入資料格式有誤');
        //            msglabel('訊息區:輸入資料格式有誤');
        //        }
        //    }
        //}, {
        //    itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        //}]
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
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
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

    //預設日期區間為最近一個月
    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year

    T1Query.getForm().findField('P1').setValue(new Date().addMonth(-1));
    T1Query.getForm().findField('P2').setValue(new Date());

});
