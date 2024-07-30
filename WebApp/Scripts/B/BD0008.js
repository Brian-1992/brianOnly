Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    var whnoComboGet = '/api/BD0008/GetWhnoCombo';
    var reportUrl = '/Report/B/BD0008.aspx';
    var reportUrl2 = '/Report/B/BD0008_2.aspx';

    //條碼分類代碼清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });


    var CONTRACNO = '';
    var RECYM = '';
    var WH_NO = '';

    var originalValue = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    function setComboData() {
        Ext.Ajax.request({
            url: whnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var whno = data.etts;
                    if (whno.length > 0) {
                        for (var i = 0; i < whno.length; i++) {
                            whnoQueryStore.add({ VALUE: whno[i].VALUE, TEXT: whno[i].TEXT });
                        }
                        T1Query.getForm().findField('P1').setValue(whno[0].VALUE);
                    }
                    T1Load();
                }

            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    function setSum() {
        T1Query.getForm().findField('label1').setValue('建議總計:');
        T1Query.getForm().findField('label2').setValue('預估總計:');
        Ext.Ajax.request({
            url: '/api/BD0008/GetSum',
            method: reqVal_g,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var sum = data.etts;
                    if (sum.length > 0) {
                        
                        T1Query.getForm().findField('label1').setValue('建議總計: ' + String(sum[0].SUM1));
                        T1Query.getForm().findField('label2').setValue('預估總計: ' + String(sum[0].SUM2));
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }


    function getCount() {
        T1Query.getForm().findField('label3').setValue('總項次:');
        Ext.Ajax.request({
            url: '/api/BD0008/GetCount',
            method: reqVal_g,
            params: {
                recym: T1Query.getForm().findField('P0').rawValue,
                wh_no: T1Query.getForm().findField('P1').getValue(),
                contracno: T1Query.getForm().findField('P2').getValue(),
            },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var count = data.etts;
                    if (count.length > 0) {
                        T1Query.getForm().findField('label3').setValue('總項次: ' + Math.round(count));
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 170;
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
                    xtype: 'monthfield',
                    fieldLabel: '年月份',
                    name: 'P0',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    name: 'P1',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: whnoQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'combobox',
                    fieldLabel: '合約',
                    name: 'P2',
                    id: 'P2',
                    queryMode: 'local',
                    displayField: 'name',
                    valueField: 'abbr',
                    width: 145,
                    store: [
                        { abbr: '0Y/0N', name: '0Y/0N' },
                        { abbr: '0N', name: '0N' },
                        { abbr: '0Y', name: '0Y' }
                    ],
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    fieldLabel: '列印排序',
                    xtype: 'radiogroup',
                    //name: 'P3',
                    items: [
                        { boxLabel: '廠商', name: 'P3',  inputValue: '1', width: 50, checked: true },
                        { boxLabel: '院內碼', name: 'P3', inputValue: '2', width: 70},
                        { boxLabel: '金額', name: 'P3', inputValue: '3',width: 50 }
                    ],
                    width: 250,
                    padding: '0 4 0 4',

                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                        }
                        else {
                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    id: 'label3',
                    name: 'label3',
                    xtype: 'displayfield',
                    value: '總項次:',
                    padding: '0 4 0 25',
                    width: 130
                },{
                    id: 'label1',
                    name: 'label1',
                    xtype: 'displayfield',
                    value: '建議總計:',
                    padding: '0 4 0 4',
                    width: 130
                }, {
                    id: 'label2',
                    name: 'label2',
                    xtype: 'displayfield',
                    value: '預估總計:',
                    padding: '0 4 0 4',
                    width: 130
                },
            ]
        }]
    })

    var T1Store = Ext.create('WEBAPP.store.BD0008', { // 定義於/Scripts/app/store/PhVender.js
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
        var f = T1Query.getForm();
        CONTRACNO = f.findField('P2').getValue();
        RECYM = f.findField('P0').rawValue;
        WH_NO = f.findField('P1').getValue();
        
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BD0008/Update';
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1print', text: '列印', disabled: true, handler: function () {
                    showReport();
                }
            }, {
                itemId: 't1print2', text: '零購品項進貨10萬以下', handler: function () {
                    showReport2();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField('x').setValue(x);
        f.findField('ESTQTY').setReadOnly(false);
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
            text: '項次'
        }, {
            text: "月份",
            dataIndex: 'RECYM',
            width: 60
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            width: 210,
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 60
        }, {
            text: "單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "建議金額",
            dataIndex: 'ADVISEMONEY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "現存量",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 65
        }, {
            text: "建議量",
            dataIndex: 'ADVISEQTY',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "<span style='color:red'>預估量</span>",
            dataIndex: 'ESTQTY',
            style: 'text-align:left',
            align: 'right',
            width: 65,
            editor: {
                xtype: 'textfield',
                maskRe: /[0-9]/,
                regexText: '只能輸入數字',
                regex: /^([1-9][0-9]*|0)$/,
                listeners: {
                    specialkey: function (field, e) {
                        if (e.getKey() === e.UP || e.getKey() === e.DOWN) {
                            var editPlugin = this.up().editingPlugin;
                            
                            
                            editPlugin.completeEdit();
                            var sm = T1Grid.getSelectionModel();

                            var rowIndex = editPlugin.context.rowIdx;
                            if (e.getKey() === e.UP) {
                                rowIndex -= 1;
                            }
                            else {
                                rowIndex += 1;
                            }

                            sm.select(rowIndex);
                            editPlugin.startEdit(rowIndex, editPlugin.context.colIdx);
                        }
                    }
                }
            }
        }, {
            text: "最小包裝量",
            dataIndex: 'UNIT_SWAP',
            style: 'text-align:left',
            align: 'right',
            width: 90
        }, {
            text: "建議金額",
            dataIndex: 'ADVISEMONEY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "單筆價",
            dataIndex: 'AMOUNT',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
                text: "合約碼",
            dataIndex: 'CONTRACNO',
            width: 40
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NO',
            width: 160
        }, {
            dataIndex: 'FLAG',
            xtype: 'hidden'
        }, {
            dataIndex: 'WH_NO',
            xtype: 'hidden'
        }, {
            header: "",
            flex: 1
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {

                    T1Grid.down('#t1print').setDisabled(true);

                    if (T1Store.getCount() > 0) {
                        T1Grid.down('#t1print').setDisabled(false);
                        setSum();
                        getCount();
                    }
                }
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: function (editor, context, eOpts)
                    {
                        originalValue = context.value;
                        context.value = null;
                    },
                    edit: function (editor, context, eOpts) {
                        
                        if (context.value == "" || context.value == originalValue) {
                            var selection = T1Grid.getSelection()[0].data;
                            var rec = T1Store.getAt(T1Grid.store.indexOf(selection));
                            rec.set('ESTQTY', originalValue);
                            rec.commit();
                        }
                        else
                            {


                            var selection = T1Grid.getSelection()[0].data;
                            var rec = T1Store.getAt(T1Grid.store.indexOf(selection));
                            rec.set('AMOUNT', T1LastRec.data.ESTQTY * T1LastRec.data.M_CONTPRICE);


                            Ext.Ajax.request({
                                url: '/api/BD0008/Update',
                                method: reqVal_p,
                                params: {
                                    RECYM: selection.RECYM,
                                    WH_NO: selection.WH_NO,
                                    MMCODE: selection.MMCODE,
                                    M_PURUN: selection.M_PURUN,
                                    M_CONTPRICE: selection.M_CONTPRICE,
                                    ADVISEQTY: selection.ADVISEQTY,
                                    STKQTY: selection.STKQTY,
                                    ESTQTY: selection.ESTQTY,
                                    CONTRACNO: selection.CONTRACNO,
                                    AMOUNT: selection.AMOUNT,
                                    INV_QTY: selection.INV_QTY
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        rec.commit();
                                        setSum();
                                    }
                                },
                                failure: function (response, options) {

                                }
                            });
                        }
                    }
                }
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                //viewport.down('#form').expand();
                //setFormT1a();
            }
        }
    });

    //function setFormT1a() {
    //    T1Grid.down('#edit').setDisabled(T1Rec === 0);
    //    if (T1LastRec) {
    //        isNew = false;
    //        T1Form.loadRecord(T1LastRec);
    //        var f = T1Form.getForm();
    //        f.findField('x').setValue('U');
    //    }
    //    else {
    //        T1Form.getForm().reset();
    //    }
    //}

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
            labelWidth: 90,
            enforceMaxLength: true,
            submitValue: true
        },
        defaultType: 'displayfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'FLAG',
            xtype: 'hidden'
        }, {
            name: 'WH_NO',
            xtype: 'hidden'
        }, {
            fieldLabel: "月份",
            name: 'RECYM',
        }, {
            fieldLabel: "院內碼",
            name: 'MMCODE',
        }, {
            fieldLabel: "藥品名稱",
            name: 'MMNAME_E',
        }, {
            fieldLabel: "申購劑量單位",
            name: 'M_PURUN',
        }, {
            fieldLabel: "合約價",
            name: 'M_CONTPRICE',
        }, {
            fieldLabel: "建議量",
            name: 'ADVISEQTY',
        }, {
            fieldLabel: "建議金額",
            name: 'ADVISEMONEY',
        }, {
            fieldLabel: "現存量",
            name: 'STKQTY',
        }, {
            xtype: "numberfield",
            fieldLabel: "預估量",
            name: 'ESTQTY',
            readOnly: 'ture',
            hideTrigger: 'true'
        }, {
            fieldLabel: "單筆價",
            name: 'AMOUNT',
        }, {
            fieldLabel: "合約",
            name: 'CONTRACNO',
        }, {
            fieldLabel: "廠商",
            name: 'AGEN_NO'
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
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "U":
                            r.data.ESTQTY = f2.findField('ESTQTY').getValue();
                            r.commit();
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
            fc.setReadOnly(true);
        });
        f.findField('ESTQTY').allowBlank = true;
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }
    function showReport() {
        if (!win) {
            
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?WH_NO=' + WH_NO
                + '&RECYM=' + RECYM
                + '&CONTRACNO=' + CONTRACNO
                + '&ORDERBY=' + T1Query.getForm().findField('P3').getGroupValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    function showReport2() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl2
                + '?WH_NO=' + WH_NO
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
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
        //    ,
        //{
        //    itemId: 'form',
        //    region: 'east',
        //    collapsible: true,
        //    collapsed: true,
        //    floatable: true,
        //    width: 300,
        //    title: '瀏覽',
        //    border: false,
        //    layout: {
        //        type: 'fit',
        //        padding: 5,
        //        align: 'stretch'
        //    },
        //    items: [T1Form]
        //}
        ]
    });

    T1Query.getForm().findField('P2').setValue('0Y/0N');
    var now = new Date();
    T1Query.getForm().findField('P0').setValue(new Date(now.getFullYear(), now.getMonth() + 1, 1));


});
