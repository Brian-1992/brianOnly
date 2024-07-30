var m_chk_no;
var T1Set = '';
var T1LastRec;
var viewModel = Ext.create('WEBAPP.store.CE0007VM');
var T1Store = viewModel.getStore('CHK_MAST');

function getColumnIndex(columns, dataIndex) {
    var index = -1;
    for (var i = 0; i < columns.length; i++) {
        if (columns[i].dataIndex == dataIndex) {
            index = i;
        }
    }

    return index;
}

function T1Load() {
    T1Store.getProxy().setExtraParam("CHK_NO", m_chk_no);
    T1Store.load(function () {
        setHeaderInfo(this.getAt(0).data);
    });
}

var T2Store = viewModel.getStore('CHK_DETAIL');
function T2Load(isFirst) {
    T2Store.getProxy().setExtraParam("CHK_NO", m_chk_no);
    if (isFirst) {
        T2Tool.moveFirst();
    } else {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }

}

function setHeaderInfo(item) {
    var f = T2Query.getForm();
    f.findField("CHK_WH_NO").setValue(item['CHK_WH_NO']);
    f.findField("CHK_YM").setValue(item['CHK_YM']);
    f.findField("CHK_PERIOD_NAME").setValue(item['CHK_PERIOD_NAME']);
    f.findField("CHK_TYPE_NAME").setValue(item['CHK_TYPE_NAME']);
    f.findField("CHK_STATUS_NAME").setValue(item['CHK_STATUS_NAME']);
    f.findField("WH_KIND_NAME").setValue(item['WH_KIND_NAME']);
    f.findField("CHK_NO").setValue(item['CHK_NO']);
    f.findField("CHK_WH_GRADE").setValue(item['CHK_WH_GRADE']);
    f.findField("CHK_PERIOD").setValue(item['CHK_PERIOD']);
    f.findField("CHK_TYPE").setValue(item['CHK_TYPE']);
    f.findField("CHK_STATUS").setValue(item['CHK_STATUS']);
    f.findField("CHK_WH_KIND").setValue(item['CHK_WH_KIND']);
    f.findField("CHK_NO1").setValue(item['CHK_NO1']);

    var wh_no = item['CHK_WH_NO'];
    if (wh_no == '560000') {
        var index1 = getColumnIndex(T2Grid.getColumns(), 'STORE_QTYC');
        var index2 = getColumnIndex(T2Grid.getColumns(), 'CHK_QTY_DIFF');
        T1Form.getForm().findField('STORE_QTYC').hide();

        Ext.getCmp('btnUpdate').hide();
        T2Grid.columns[index1].hide();
        T2Grid.columns[index2].hide();
    } else {
        var index1 = getColumnIndex(T2Grid.getColumns(), 'STORE_QTYC');
        var index2 = getColumnIndex(T2Grid.getColumns(), 'CHK_QTY_DIFF');
        T1Form.getForm().findField('STORE_QTYC').show();

        Ext.getCmp('btnUpdate').show();
        T2Grid.columns[index1].show();
        T2Grid.columns[index2].show();
    }

    T2Tool.down('#btnClose').hide();
    if (Number(f.findField("CHK_WH_GRADE").getValue()) > 2 &&
        f.findField("CHK_WH_KIND").getValue() == '0' &&
        f.findField('CHK_PERIOD').getValue() == 'D' &&
        f.findField('CHK_TYPE').getValue() != '3' &&
        f.findField('CHK_TYPE').getValue() != '4'
    ) {
        T2Tool.down('#btnClose').show();
    }

    if ((f.findField("CHK_WH_NO").getValue() == "ANE1" || f.findField("CHK_WH_NO").getValue() == "OR1") &&
        f.findField('CHK_PERIOD').getValue() == 'D') {
        T2Tool.down('#btnClose').show();
    }

    var chk_status = item['CHK_STATUS'];
    if (chk_status == '2') {
        T2Tool.down('#btnUpdate').enable();
        T2Tool.down('#btnFinish').enable();
        T2Tool.down('#btnClose').disable();
    } else if (chk_status == '3') {
        T2Tool.down('#btnUpdate').disable();
        T2Tool.down('#btnFinish').disable();
        T2Tool.down('#btnClose').enable();
    }
    else {
        T2Tool.down('#btnUpdate').disable();
        T2Tool.down('#btnFinish').disable();
        T2Tool.down('#btnClose').disable();
    }

    // 檢查此盤點單是否為本次月結期間開立，否 => 不可做任何異動
    var myMask = new Ext.LoadMask(T1Panel, { msg: '檢查月結年月' });
    myMask.show();
    Ext.Ajax.request({
        url: '/api/CE0004/CheckCurrentYm',
        method: reqVal_p,
        params: {
            CHK_NO: m_chk_no,
        },
        success: function (response) {
            var data = Ext.decode(response.responseText);
            if (data.success) {

                myMask.hide();

                if (data.msg == 'N') {
                    T2Tool.down('#btnUpdate').disable();
                    T2Tool.down('#btnFinish').disable();
                    T2Tool.down('#btnClose').disable();
                }

            }
            else {

                myMask.hide();
                Ext.MessageBox.alert('錯誤', data.msg);
            }

        },
        failure: function (response) {
            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            myMask.hide();
        }
    });
}

function close() {
    var myMask = new Ext.LoadMask(T1Panel, { msg: '處理中...' });
    myMask.show();
    Ext.MessageBox.confirm('訊息', '確認進行過帳調整, 並結案?', function (btn, text) {
        if (btn === 'yes') {
            var f = T2Query.getForm();
            Ext.Ajax.request({
                url: '/api/CE0004/Close',
                method: reqVal_p,
                params: {
                    CHK_NO1: T2Query.getForm().findField('CHK_NO1').getValue(),
                    //CHK_YM: f.findField('CHK_YM').getValue()
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        //T2Store.removeAll();
                        //T1Grid.getSelectionModel().deselectAll();
                        //T1Load();
                        //Ext.getCmp('btnSubmit').setDisabled(true);

                        var tp = T1Panel.up('#rootTabs'); // CE0004.js Tabs的itemId
                        var activeTab = tp.getActiveTab();
                        var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                        tp.remove(activeTabIndex);
                        msglabel('完成過帳調整並結案成功');

                    }
                    else {
                        myMask.hide();
                        Ext.MessageBox.alert('錯誤', data.msg);
                    }
                },
                failure: function (response) {
                    myMask.hide();
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                }
            });
        } else {
            myMask.hide();
        }
    });
}

// 查詢欄位
var mLabelWidth = 80;
var mWidth = 230;
var T2Query = Ext.widget({
    id: 'aa',
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
    items: [
        {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            //layout: {
            //    type: 'table',
            //    columns: 5
            //},
            //padding: '0 0 5 0',
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房代碼',
                    name: 'CHK_WH_NO',
                    width: 170,
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點年月日',
                    name: 'CHK_YM',
                    width: 150,
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點期',
                    name: 'CHK_PERIOD_NAME',
                    width: 150,
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點類別',
                    name: 'CHK_TYPE_NAME',
                    width: 180,
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '狀態',
                    name: 'CHK_STATUS_NAME',
                    width: 150,
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房分類',
                    name: 'WH_KIND_NAME',
                    width: 170,
                    hidden: true
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '盤點單號',
                    name: 'CHK_NO',
                    width: 200
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '初盤點單號',
                    name: 'CHK_NO1',
                    width: 250,
                    hidden: true
                },
                {
                    fieldLabel: 'CHK_WH_GRADE',
                    name: 'CHK_WH_GRADE',
                    xtype: 'hidden'
                },
                {
                    fieldLabel: 'CHK_PERIOD',
                    name: 'CHK_PERIOD',
                    xtype: 'hidden'
                },
                {
                    fieldLabel: 'CHK_TYPE',
                    name: 'CHK_TYPE',
                    xtype: 'hidden'
                },
                {
                    fieldLabel: 'CHK_STATUS',
                    name: 'CHK_STATUS',
                    xtype: 'hidden'
                },
                {
                    fieldLabel: 'CHK_WH_KIND',
                    name: 'CHK_WH_KIND',
                    xtype: 'hidden'
                }
                //{
                //    xtype: 'displayfield',
                //    fieldLabel: '盤點單負責人',
                //    value: session['UserName'],
                //    width: 170,
                //    padding: '0 4 0 4'
                //}

            ]
        }
    ]
});

var T2Tool = Ext.create('Ext.PagingToolbar', {
    store: T2Store,
    displayInfo: true,
    border: false,
    plain: true,
    buttons: [
        {
            text: '修改',
            itemId: 'btnUpdate',
            name: 'btnUpdate',
            id: 'btnUpdate',
            disabled: true,
            handler: function () {
                T1Set = '/api/CE0007/DetailUpdate';
                setFormT1('U', '修改');
            }
        },
        {
            text: '完成盤點調整',
            itemId: 'btnFinish',
            name: 'btnFinish',
            handler: function () {
                //T1Set = '/api/CE0007/DetailUpdate';
                //setFormT1('U', '修改');
                Ext.MessageBox.confirm('訊息', '確認完成盤點調整?', function (btn, text) {
                    if (btn === 'yes') {
                        var myMask = new Ext.LoadMask(T1Panel, { msg: '處理中...' });
                        myMask.show();

                        var f = T2Query.getForm();
                        Ext.Ajax.timeout = 300000;
                        Ext.Ajax.setTimeout(300000);
                        Ext.override(Ext.data.proxy.Server, { timeout: Ext.Ajax.timeout });
                        Ext.override(Ext.data.Connection, { timeout: Ext.Ajax.timeout });
                        Ext.Ajax.request({
                            url: '/api/CE0007/Finish',
                            method: reqVal_p,
                            params: {
                                CHK_NO: m_chk_no,
                                CHK_NO1: f.findField('CHK_NO1').getValue(),
                                CHK_YM: f.findField('CHK_YM').getValue()
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    myMask.hide();
                                    msglabel('完成盤點調整成功');
                                    // 過帳結案按鈕為隱藏狀態，直接回到第一頁清單
                                    if (Ext.getCmp('btnClose').hidden) {
                                        var tp = T1Panel.up('#rootTabs'); // CE0004.js Tabs的itemId
                                        var activeTab = tp.getActiveTab();
                                        var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                                        tp.remove(activeTabIndex);
                                        return;
                                    }
                                    // 過帳結案按鈕為顯示狀態，依回傳資料設定按鈕狀態
                                    setHeaderInfo(data.etts[0]);
                                    close();
                                }
                                else {
                                    Ext.MessageBox.alert('錯誤', data.msg);
                                    myMask.hide();
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                myMask.hide();
                            }
                        });
                    }
                });
            }
        },
        {
            text: '過帳與結案',
            itemId: 'btnClose',
            name: 'btnClose',
            id: 'btnClose',
            disabled: true,
            handler: function () {
                close();
            }
        },
    ]
});

var T2Grid = Ext.create('Ext.grid.Panel', {
    //title: T1Name,
    id: (this.chk_no + 't1Grid'),
    store: T2Store,
    plain: true,
    loadingText: '處理中...',
    loadMask: true,
    cls: 'T1',
    //plugins: [T1RowEditing],
    dockedItems: [

        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
    ],
    //selModel: {
    //    checkOnly: false,
    //    injectCheckbox: 'first',
    //    mode: 'MULTI'
    //},
    //selType: 'checkboxmodel',
    columns: [
        {
            xtype: 'rownumberer'
        },
        {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        },
        {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200
        },
        {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200
        },
        {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 130,
            //editor: {
            //    xtype: 'combo',
            //    store: T3Store,
            //    displayField: 'WH_NAME',
            //    valueField: 'WH_NO'
            //}
        },
        {
            text: "儲位名稱",
            dataIndex: 'LOC_NAME',
            width: 130,
            //editor: {
            //    xtype: 'combo',
            //    store: T3Store,
            //    displayField: 'WH_NAME',
            //    valueField: 'WH_NO'
            //}
        },
        {
            text: "計量單位",
            //dataIndex: 'M_PURUN',
            dataIndex: 'BASE_UNIT',
            width: 100,
        },
        {
            text: "電腦量",
            dataIndex: 'STORE_QTYC',
            width: 70,
            align: 'right',
            style: 'text-align:left'
             
        },
        {
            text: "盤點量",
            dataIndex: 'CHK_QTY',
            width: 70,
            align: 'right',
            style: 'text-align:left',
        },
        {
            text: "盤差",
            dataIndex: 'CHK_QTY_DIFF',
            width: 70,
            align: 'right',
            style: 'text-align:left',
        },
        //{
        //    text: "狀態",
        //    dataIndex: 'STATUS_INI',
        //    width: 80,
        //},
        {
            text: "附註",
            dataIndex: 'CHK_REMARK',
            width: 300,
        },

        {
            header: "",
            flex: 1
        }
    ],
    listeners: {
        click: {
            element: 'el',
            fn: function () {
                if (T1Form.hidden === true) {
                    T1Form.setVisible(true);
                }


                // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                if (T1LastRec != null) {
                    T2Tool.down('#btnUpdate').setDisabled(false);

                    var status = T2Query.getForm().findField('CHK_STATUS').getValue();
                    if (status == '2') {
                        T2Tool.down('#btnUpdate').setDisabled(false);
                        T2Tool.down('#btnFinish').setDisabled(false);
                        //T2Tool.down('#btnClose').setDisabled(false);
                    }
                    else {
                        T2Tool.down('#btnUpdate').setDisabled(true);
                        T2Tool.down('#btnFinish').setDisabled(true);
                        //T2Tool.down('#btnClose').setDisabled(true);
                    }

                    if (T1Set === '')
                        setFormT1a();

                    //T2Load();
                }

            }
        },
        selectionchange: function (model, records) {
            T1Rec = records.length;
            T1LastRec = records[0];
            setFormT1a();

            if (T1LastRec == null) {
                T2Tool.down('#btnUpdate').setDisabled(true);
            }
        }
    }
});

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
        {
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },
        {
            fieldLabel: 'CHK_NO',
            name: 'CHK_NO',
            xtype: 'hidden'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '儲位',
            name: 'STORE_LOC',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '儲位名稱',
            name: 'LOC_NAME',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '計量單位',
            //name: 'M_PURUN',
            name: 'BASE_UNIT',
            submitValue: true,
        },
        {
            xtype: 'displayfield',
            fieldLabel: '電腦量',
            name: 'STORE_QTYC',
            submitValue: true,
        },
        {
            fieldLabel: '盤點量',
            name: 'CHK_QTY',
            fieldCls: 'required',
            allowBlank: false, // 欄位為必填
            submitValue: true,
            readOnly: true
        },
        //{
        //    fieldLabel: '條碼編號',
        //    name: 'BARCODE',
        //    enforceMaxLength: true,
        //    maxLength: 50,
        //    submitValue: true,
        //    readOnly: true
        //},
        {
            xtype: 'textareafield',
            fieldLabel: '附註',
            name: 'CHK_REMARK',
            enforceMaxLength: true,
            maxLength: 100,
            submitValue: true,
            readOnly: true
        },
    ],
    buttons: [
        {
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = T1Form.up('#form_1').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                            T1Set = '';
                        }
                    });
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
            }
        },
        {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }
    ]
});

function T1Submit() {
    var f = T1Form.getForm();
    if (f.isValid()) {
        var myMask = new Ext.LoadMask(T1Form.up('#form_1'), { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                myMask.hide();
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                switch (f2.findField("x").getValue()) {
                    case "I":
                        //T1Query.getForm().reset();
                        //var v = action.result.etts[0];
                        //T2Store.removeAll();
                        //T1Query.getForm().findField('P1').setValue(v.DOCNO);
                        T1Load();
                        msglabel('訊息區:資料新增成功');
                        break;
                    case "U":
                        T2Load(false);
                        msglabel('資料更新成功');
                        break;
                    case "R":
                        T1Load();
                        msglabel('訊息區:資料退回成功');
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
    T1Set = '';
    T1Panel.up('#t1Grid_1').unmask();
    T1Form.up('#form_1').collapse();
    var f = T1Form.getForm();
    f.reset();
    f.getFields().each(function (fc) {
        if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
            fc.setReadOnly(true);
        } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
            fc.setReadOnly(true);
        }
    });
    T1Form.down('#cancel').hide();
    T1Form.down('#submit').hide();
    T1Form.up('#form_1').setTitle('瀏覽');
    //setFormT1a();
}

var T1Panel = Ext.create('Ext.panel.Panel', {
    xtype: 'container',
    items: [T2Query, T2Grid]
});

// 按'新增'或'修改'時的動作
function setFormT1(x, t) {
    T1Panel.up('#t1Grid_1').mask();
    //viewport.down('#t2Grid').mask();
    T1Form.up('#form_1').setTitle(t);
    T1Form.up('#form_1').expand();
    //Tabs.setActiveTab('Form');
    //var currentTab = Tabs.getActiveTab();
    //currentTab.setTitle('填寫');

    var f = T1Form.getForm();

    f.findField('x').setValue(x);
    f.findField('CHK_NO').setValue(T2Query.getForm().findField('CHK_NO').getValue());
    f.findField('CHK_QTY').setReadOnly(false);
    f.findField('CHK_REMARK').setReadOnly(false);

    T1Form.down('#cancel').setVisible(true);
    T1Form.down('#submit').setVisible(true);
    //u.focus();
}

// 點選T1Grid一筆資料的動作
function setFormT1a() {
    if (T1LastRec != null) {
        T1Form.up('#form_1').expand();
        T1Form.up('#form_1').setTitle('瀏覽');

        var f = T1Form.getForm();
        f.findField("MMCODE").setValue(T1LastRec.data["MMCODE"]);
        f.findField("MMNAME_C").setValue(T1LastRec.data["MMNAME_C"]);
        f.findField("MMNAME_E").setValue(T1LastRec.data["MMNAME_E"]);
        f.findField("STORE_LOC").setValue(T1LastRec.data["STORE_LOC"]);
        f.findField("LOC_NAME").setValue(T1LastRec.data["LOC_NAME"]);
        //f.findField("M_PURUN").setValue(T1LastRec.data["M_PURUN"]);
        f.findField("BASE_UNIT").setValue(T1LastRec.data["BASE_UNIT"]);
        f.findField("STORE_QTYC").setValue(T1LastRec.data["STORE_QTYC"]);
        f.findField("CHK_QTY").setValue(T1LastRec.data["CHK_QTY"]);
        f.findField("CHK_REMARK").setValue(T1LastRec.data["CHK_REMARK"]);

        T1Form.down('#cancel').setVisible(false);
        T1Form.down('#submit').setVisible(false);
    }

}
function setHeight() {
    
    Ext.getCmp((this.chk_no + 't1Grid')).setHeight($(window).height() - 103);
}

Ext.define('TAB.C.CE0007_INI', {
    extend: 'Ext.panel.Panel',
    requires: [
        //'FaZang.view.Menu.AddItemController',
        //'FaZang.view.Menu.AddItemViewModel'
    ],
    //autoScroll: true,
    layout: {
        type: 'border',
        padding: 0
    },
    defaults: {
        split: true
    },
    initComponent: function () {
        m_chk_no = this.chk_no;
        T1Load();
        T2Load(true);
        T1LastRec = null;
        var f = T1Form.getForm();
        f.reset();
        setFormT1a();
        this.callParent(arguments);
        setHeight();
    },
    items: [
        {
            itemId: 't1Grid_1',
            region: 'center',
            //layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Panel]
            //itemId: 't1top',
            //region: 'center',
            //layout: 'border',
            //collapsible: false,
            //title: '',
            //border: false,
            //items: [
            //    {
            //        itemId: 't1Grid',
            //        region: 'center',
            //        layout: 'fit',
            //        collapsible: false,
            //        title: '',
            //        border: false,
            //        //height: '90%',
            //        items: [T1Panel]
            //    }
            //]
        },
        {
            itemId: 'form_1',
            region: 'east',
            collapsible: true,
            //floatable: true,
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

Ext.on('resize', function () {
    setHeight();
});
