Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 參數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // var T1Get = '/api/CC0003/All'; // 查詢(改為於store定義)
        var T1Set = ''; // 新增/修改/刪除
        var T2Set = ''; // 新增/修改/刪除
        //var T1Name = "進貨驗收確認";

        var GetWH_NO = '../../../api/CC0003/GetWH_NO';
        var GetAGEN_NO = '../../../api/CC0003/GetAGEN_NO';
        var CallMM_PO_INREC_CHK = '../../../api/CC0003/CallMM_PO_INREC_CHK';
        var reportUr = '/Report/C/CC0003.aspx';
        var T1GetExcel = '/api/CC0003/Excel';
        var T1Rec = 0;
        var T1LastRec = null;
        var Temp = null;
        var rec = null;
        var T2Rec = 0;
        var T2LastRec = null;
        var Temp2 = null;
        var rec2 = null;

        var T3Rec = 0;
        var T3LastRec = null;
        var Temp3 = null;
        var rec3 = null;
        
        var first_wh_no = '';

        //新增庫別的store
        var WH_NO_Store = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        //新增廠商碼的store
        var AGEN_NO_Store = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        //建立查詢庫別的下拉式選單
        function SetWH_NO() {
            Ext.Ajax.request({
                url: GetWH_NO,
                method: reqVal_g,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        var wh_nos = data.etts;
                        if (wh_nos.length > 0) {
                            for (var i = 0; i < wh_nos.length; i++) {
                                WH_NO_Store.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                                first_wh_no = wh_nos[0].VALUE;
                                T1QueryForm.getForm().findField("P0").setValue(wh_nos[0].VALUE);
                                T2QueryForm.getForm().findField("P4").setValue(wh_nos[0].VALUE);
                                T3QueryForm.getForm().findField("P0").setValue(wh_nos[0].VALUE);
                            }
                        }
                    }
                },
                failure: function (response, options) {
                }
            });
        }
        function showReport() {
            if (!win) {
                var winform = Ext.create('Ext.form.Panel', {
                    id: 'iframeReport',
                    layout: 'fit',
                    closable: false,
                    html: '<iframe src="' + reportUr
                    + '?p0=' + T1QueryForm.getForm().findField('P0').getValue()
                    + '&p1=' + T1QueryForm.getForm().findField('P1').getValue()
                    + '&p2=' + T1QueryForm.getForm().findField('P2').getRawValue()
                    + '&p2_1=' + T1QueryForm.getForm().findField('P2_1').getRawValue()
                    + '&p3=' + T1QueryForm.getForm().findField('P3').getValue()
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                    buttons: [{
                        text: '關閉',
                        margin: '0 20 30 0',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }]
                });
                var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
            }
            win.show();
        }
        //建立查詢廠商碼的下拉式選單
        function SetAGEN_NO() {
            AGEN_NO_Store.add({ VALUE: '', TEXT: '全部' });
            AGEN_NO_Store.add({ VALUE: '0', TEXT: '001 ~ 100' });
            AGEN_NO_Store.add({ VALUE: '1', TEXT: '101 ~ 200' });
            AGEN_NO_Store.add({ VALUE: '2', TEXT: '201 ~ 300' });
            AGEN_NO_Store.add({ VALUE: '3', TEXT: '301 ~ 400' });
            AGEN_NO_Store.add({ VALUE: '4', TEXT: '401 ~ 500' });
            AGEN_NO_Store.add({ VALUE: '5', TEXT: '501 ~ 600' });
            AGEN_NO_Store.add({ VALUE: '6', TEXT: '601 ~ 700' });
            AGEN_NO_Store.add({ VALUE: '7', TEXT: '701 ~ 800' });
            AGEN_NO_Store.add({ VALUE: '8', TEXT: '801 ~ 900' });
            AGEN_NO_Store.add({ VALUE: '9', TEXT: '901 ~ 999' });
            T1QueryForm.getForm().findField("P1").setValue("");
            T2QueryForm.getForm().findField("P5").setValue("");
            T3QueryForm.getForm().findField("P1").setValue("");
        }
    }
    //#region T1
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T1 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        //新增類別的store
        var KIND_Store = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        //建立查詢類別的下拉式選單
        function SetKIND() {
            KIND_Store.add({ VALUE: '', TEXT: '全部' });
            KIND_Store.add({ VALUE: 'N', TEXT: '未進貨' });
            KIND_Store.add({ VALUE: 'Y', TEXT: '已進貨' });
            T1QueryForm.getForm().findField("P3").setValue('N');

            T3QueryForm.getForm().findField("P3").setValue('');
        }

        // T1 查詢欄位
        var T1QueryForm = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true,
            defaultType: 'textfield',
            fieldDefaults: {
                labelAlign: 'right',
                msgTarget: 'side',
                labelWidth: 70,
                width: 180
            },
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '庫別',
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 50,
                            width: 200,
                            store: WH_NO_Store,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            anyMatch: true,
                            fieldCls: 'required',
                            allowBlank: false, // 欄位是否為必填
                            blankText: "請輸入庫別",
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '廠商代碼',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: 70,
                            width: 220,
                            store: AGEN_NO_Store,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            anyMatch: true,
                            allowBlank: true, // 欄位是否為必填
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        //}, {
                        //    xtype: 'datefield',
                        //    fieldLabel: '採購日期',
                        //    name: 'P2',
                        //    id: 'P2',
                        //    labelWidth: 70,
                        //    width: 170,
                        //    value: new Date(),
                        //    enforceMaxLength: true,
                        //    fieldCls: 'required',
                        //    allowBlank: false, // 欄位是否為必填
                        //    blankText: "請輸入採購日期",
                        //    renderer: function (value, meta, record) {
                        //        return Ext.util.Format.date(value, 'X/m/d');
                        //    }
                        //}, {
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '採購日期',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 70,
                            width: 150,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            vtype: 'dateRange',
                            dateRange: { end: 'P2_1' },
                            value: Ext.Date.add(new Date(), Ext.Date.DAY, -3)
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P2_1',
                            id: 'P2_1',
                            labelWidth: 8,
                            width: 88,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                            fieldCls: 'required',
                            vtype: 'dateRange',
                            dateRange: { begin: 'P2' },
                            value: new Date()
                        }, {
                            xtype: 'combo',
                            fieldLabel: '類別',
                            name: 'P3',
                            id: 'P3',
                            labelWidth: 50,
                            width: 150,
                            store: KIND_Store,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            anyMatch: true,
                            fieldCls: 'required',
                            allowBlank: false, // 欄位是否為必填
                            blankText: "請輸入類別",
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'button',
                            itemId: 'query',
                            text: '查詢',
                            handler: function () {
                                T1Load();
                                msglabel("");
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'clean',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                T1QueryForm.getForm().findField("P0").setValue(first_wh_no);
                                T1QueryForm.getForm().findField("P1").setValue('');
                                T1QueryForm.getForm().findField("P2").setValue(Ext.Date.add(new Date(), Ext.Date.DAY, -3));
                                T1QueryForm.getForm().findField("P2_1").setValue(new Date());
                                T1QueryForm.getForm().findField("P3").setValue('');
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }]
                }]
        });

        // T1 Store
        var T1Store = Ext.create('WEBAPP.store.CC0003_1', { // 定義於/Scripts/app/store/CC0003_1.js
            listeners: {
                beforeload: function (store, options) {
                    store.removeAll();
                    // 載入前將查詢條件P0~P2的值代入參數
                    var np = {
                        p0: T1QueryForm.getForm().findField('P0').getValue(),
                        p1: T1QueryForm.getForm().findField('P1').getValue(),
                        p2: T1QueryForm.getForm().findField('P2').getRawValue(),
                        p2_1: T1QueryForm.getForm().findField('P2_1').getRawValue(),
                        p3: T1QueryForm.getForm().findField('P3').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        // T1 toolbar,包含換頁、新增/修改/刪除鈕
        var T1Tool = Ext.create('Ext.PagingToolbar', {
            store: T1Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    itemId: 'T1add', text: '新增', disabled: true, hidden:true, handler: function () {
                        T1Set = '../../../api/CC0003/Insert';
                        SetSubmitT1('I');
                    }
                },
                {
                    itemId: 'T1commit', text: '儲存', disabled: true, handler: function () {
                        T1Set = '../../../api/CC0003/Commit';
                        T1CheckQty('C');
                    }
                },
                {
                    itemId: 'T1delete', text: '退貨', disabled: true,
                    handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('退貨', '是否確定執行退貨？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/CC0003/Back';
                                T1Submit('B');
                            }
                        });
                    }
                },
                {
                    itemId: 'T1in_pur', text: '進貨', disabled: true,
                    handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('提醒', "是否確定執行進貨,增加庫存量？", function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/CC0003/Commit';
                                T1CheckQty('P');
                                //T1in_pur();
                            }
                        });
                    }
                }, {
                    itemId: 'export', text: '匯出', 
                    handler: function () {
                        var p = new Array();
                        p.push({ name: 'p0', value: T1QueryForm.getForm().findField('P0').getValue() }); //SQL篩選條件
                        p.push({ name: 'p1', value: T1QueryForm.getForm().findField('P1').getValue() }); //SQL篩選條件
                        p.push({ name: 'p2', value: T1QueryForm.getForm().findField('P2').getRawValue() }); //SQL篩選條件
                        p.push({ name: 'p2_1', value: T1QueryForm.getForm().findField('P2_1').getRawValue() }); //SQL篩選條件
                        p.push({ name: 'p3', value: T1QueryForm.getForm().findField('P3').getValue() }); //SQL篩選條件
                        PostForm(T1GetExcel, p);
                    }
                }, {
                    itemId: 't1print', text: '列印', handler: function () {
                        showReport();
                    }
                }]
        });

        // T1 查詢結果資料列表
        var T1Grid = Ext.create('Ext.grid.Panel', {
            //title: '',
            store: T1Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T2',
            dockedItems: [
                {
                    items: [T1QueryForm]
                }, {
                    dock: 'top',
                    xtype: 'toolbar',
                    items: [T1Tool]
                }],
            selModel: Ext.create('Ext.selection.CheckboxModel', {
                checkOnly: true,
                injectCheckbox: 'first',
                mode: 'SIMPLE',
                showHeaderCheckbox: false
            }),
            columns: [{
                dataIndex: 'ACCOUNTDATE_REF',
                text: "原進貨日期",
                hidden: true
            }, {
                dataIndex: 'DELI_QTY_REF,',
                text: "原進貨量",
                hidden: true
            }, {
                dataIndex: 'LOT_NO_REF',
                text: "原批號",
                hidden: true
            }, {
                dataIndex: 'EXP_DATE_REF',
                text: "原效期",
                hidden: true
            }, {
                dataIndex: 'MEMO_REF',
                text: "原備註",
                hidden: true
            }, {
                dataIndex: 'SEQ',
                text: "流水號",
                hidden: true
            }, {
                dataIndex: 'PO_NO_REF',
                text: "採購單號",
                hidden: true
            }, {
                dataIndex: 'M_DISCPERC',
                text: "折讓比",
                hidden: true
            }, {
                dataIndex: 'UNIT_SWAP',
                text: "轉換率",
                hidden: true
            }, {
                dataIndex: 'UPRICE',
                text: "最小單價",
                hidden: true
            }, {
                dataIndex: 'DISC_CPRICE',
                text: "優惠合約單價",
                hidden: true
            }, {
                dataIndex: 'DISC_UPRICE',
                text: "優惠最小單價",
                hidden: true
            }, {
                dataIndex: 'TRANSKIND',
                text: "異動類別",
                hidden: true
            }, {
                dataIndex: 'IFLAG',
                text: "新增識別",
                hidden: true
            }, {
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            }, {
                text: "疫苗",
                dataIndex: 'VACCINE',
                width: 40,
                sortable: true
            }, {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 50,
                sortable: true
            }, {
                text: "案別",
                dataIndex: 'E_PURTYPE',
                width: 40,
                sortable: true
            }, {
                text: "採購日期",
                dataIndex: 'PURDATE',
                width: 60,
                sortable: true
            }, {
                text: "採購單號",
                dataIndex: 'PO_NO',
                width: 90,
                sortable: true
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO_NAME',
                width: 150,
                sortable: true
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70,
                sortable: true
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150,
                sortable: true
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200,
                sortable: true
            }, {
                xtype: 'datecolumn',
                text: "進貨日期",
                dataIndex: 'ACCOUNTDATE',
                width: 70,
                sortable: true,
                format: 'Xmd',
                editor: {
                    xtype: 'datefield',
                    allowBlank: false,
                    format: 'Xmd',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            
                            if ((Ext.util.Format.date(newVal, 'Xmd') != T1LastRec.data.ACCOUNTDATE_REF) ||
                                T1LastRec.data.DELI_QTY != T1LastRec.data.DELI_QTY_REF ||
                                T1LastRec.data.MEMO != T1LastRec.data.MEMO_REF ||
                                T1LastRec.data.LOT_NO != T1LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T1LastRec.data.EXP_DATE, 'Xmd') != T1LastRec.data.EXP_DATE_REF) {
                                T1Grid.getSelectionModel().select(T1LastRec, true, true);

                                

                                var records = T1Grid.getSelection();
                                for (var i = 0; i < records.length; i++) {
                                    
                                    if (records[i].data.STATUS == 'N') { // 未進貨
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                    }
                                    if (records[i].data.STATUS == 'Y') { // 已進貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                    if (records[i].data.STATUS == 'E') { // 已退貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                }
                                
                            }
                            else {
                                T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                                if (T1Grid.getSelection().length == 0) {
                                    T1Grid.down('#T1commit').setDisabled(true);
                                    T1Grid.down('#T1delete').setDisabled(true);
                                    T1Grid.down('#T1in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "採購量",
                dataIndex: 'PO_QTY',
                width: 80,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                format: '0'
            }, {
                text: "<span style='color:red'>進貨量</span>",
                dataIndex: 'DELI_QTY',
                width: 80,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                minValue: 0,
                allowBlank: false,
                format: '0',
                editor: {
                    xtype: 'numberfield',
                    hideTrigger: true,
                    allowBlank: false,
                    listeners: {
                        blur: function (field, event, eOpts) {
                            if (field.value == null) {
                                rec = T1Store.getAt(T1Rec);
                                rec.set('DELI_QTY', 0);
                                rec.set('PO_AMT', 0);
                                rec.commit();
                                msglabel("進貨量<span style='color:red'>不可為空</span>");
                            }
                        },
                        change: function (field, newVal, oldVal) {
                            if (newVal > T1LastRec.data.PO_QTY) {
                                T1Grid.getView().refresh();
                                msglabel("進貨量<span style='color:red'>不可</span>超過[採購量]");
                            }
                            else {
                                if (((newVal != T1LastRec.data.DELI_QTY_REF) && (newVal <= T1LastRec.data.PO_QTY)) ||
                                    Ext.util.Format.date(T1LastRec.data.ACCOUNTDATE, 'Xmd') != T1LastRec.data.ACCOUNTDATE_REF ||
                                    T1LastRec.data.MEMO != T1LastRec.data.MEMO_REF ||
                                    T1LastRec.data.LOT_NO != T1LastRec.data.LOT_NO_REF ||
                                    Ext.util.Format.date(T1LastRec.data.EXP_DATE, 'Xmd') != T1LastRec.data.EXP_DATE_REF) {
                                    T1Grid.getSelectionModel().select(T1LastRec, true, true);

                                    var records = T1Grid.getSelection();
                                    for (var i = 0; i < records.length; i++) {
                                        
                                        if (records[i].data.STATUS == 'N') { // 未進貨
                                            T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                        }
                                        if (records[i].data.STATUS == 'Y') { // 已進貨
                                            T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                            T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                        }
                                        if (records[i].data.STATUS == 'E') { // 已退貨
                                            T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                            T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                            T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                        }
                                    }

                                    rec = T1Store.getAt(T1Rec);
                                    rec.set('PO_AMT', T1LastRec.data.PO_PRICE * newVal);
                                    rec.set('DELI_QTY', newVal);
                                    rec.commit();
                                }
                                else {
                                    T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                                    if (T1Grid.getSelection().length == 0) {
                                        T1Grid.down('#T1commit').setDisabled(true);
                                        T1Grid.down('#T1delete').setDisabled(true);
                                        T1Grid.down('#T1in_pur').setDisabled(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }, {
                text: "進貨",
                dataIndex: 'INFLAG',
                width: 50,
                sortable: true
            }, {
                text: "退貨",
                dataIndex: 'OUTFLAG',
                width: 50,
                sortable: true
            }, {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 70,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                format: '0.00'
            }, {
                text: "總金額",
                dataIndex: 'PO_AMT',
                width: 80,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                format: '0.00'
            }, {
                text: "進貨單位",
                dataIndex: 'M_PURUN',
                width: 70,
                sortable: true
            }, {
                text: "<span style='color:red'>批號</span>",
                dataIndex: 'LOT_NO',
                width: 120,
                sortable: true,
                editor: {
                    xtype: 'textfield',
                    maxLength: 20,
                    //hideTrigger: true,
                    //allowBlank: false,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (newVal != T1LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T1LastRec.data.ACCOUNTDATE, 'Xmd') != T1LastRec.data.ACCOUNTDATE_REF ||
                                T1LastRec.data.MEMO != T1LastRec.data.MEMO_REF ||
                                T1LastRec.data.DELI_QTY != T1LastRec.data.DELI_QTY_REF ||
                                Ext.util.Format.date(T1LastRec.data.EXP_DATE, 'Xmd') != T1LastRec.data.EXP_DATE_REF) {
                                T1Grid.getSelectionModel().select(T1LastRec, true, true);

                                var records = T1Grid.getSelection();
                                for (var i = 0; i < records.length; i++) {
                                    
                                    if (records[i].data.STATUS == 'N') { // 未進貨
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                    }
                                    if (records[i].data.STATUS == 'Y') { // 已進貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                    if (records[i].data.STATUS == 'E') { // 已退貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                }
                            }
                            else {
                                T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                                if (T1Grid.getSelection().length == 0) {
                                    T1Grid.down('#T1commit').setDisabled(true);
                                    T1Grid.down('#T1delete').setDisabled(true);
                                    T1Grid.down('#T1in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                xtype: 'datecolumn',
                text: "<span style='color:red'>效期</span>",
                dataIndex: 'EXP_DATE',
                width: 100,
                sortable: true,
                format: 'Xmd',
                editor: {
                    xtype: 'datefield',
                    allowBlank: false,
                    format: 'Xmd',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if ((Ext.util.Format.date(newVal, 'Xmd') != T1LastRec.data.EXP_DATE_REF) ||
                                T1LastRec.data.DELI_QTY != T1LastRec.data.DELI_QTY_REF ||
                                T1LastRec.data.MEMO != T1LastRec.data.MEMO_REF ||
                                T1LastRec.data.LOT_NO != T1LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T1LastRec.data.ACCOUNTDATE, 'Xmd') != T1LastRec.data.ACCOUNTDATE_REF) {
                                T1Grid.getSelectionModel().select(T1LastRec, true, true);

                                var records = T1Grid.getSelection();
                                for (var i = 0; i < records.length; i++) {
                                    
                                    if (records[i].data.STATUS == 'N') { // 未進貨
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                    }
                                    if (records[i].data.STATUS == 'Y') { // 已進貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                    if (records[i].data.STATUS == 'E') { // 已退貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                }
                            }
                            else {
                                T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                                if (T1Grid.getSelection().length == 0) {
                                    T1Grid.down('#T1commit').setDisabled(true);
                                    T1Grid.down('#T1delete').setDisabled(true);
                                    T1Grid.down('#T1in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "<span style='color:red'>備註</span>",
                dataIndex: 'MEMO',
                width: 120,
                sortable: true,
                editor: {
                    xtype: 'textfield',
                    hideTrigger: true,
                    //allowBlank: false,
                    maxLength: 300,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (newVal != T1LastRec.data.MEMO_REF ||
                                Ext.util.Format.date(T1LastRec.data.ACCOUNTDATE, 'Xmd') != T1LastRec.data.ACCOUNTDATE_REF ||
                                T1LastRec.data.LOT_NO != T1LastRec.data.LOT_NO_REF ||
                                T1LastRec.data.DELI_QTY != T1LastRec.data.DELI_QTY_REF ||
                                Ext.util.Format.date(T1LastRec.data.EXP_DATE, 'Xmd') != T1LastRec.data.EXP_DATE_REF) {
                                T1Grid.getSelectionModel().select(T1LastRec, true, true);

                                var records = T1Grid.getSelection();
                                for (var i = 0; i < records.length; i++) {
                                    
                                    if (records[i].data.STATUS == 'N') { // 未進貨
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                    }
                                    if (records[i].data.STATUS == 'Y') { // 已進貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                    if (records[i].data.STATUS == 'E') { // 已退貨
                                        T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                                        T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                                        T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                                    }
                                }
                            }
                            else {
                                T1Grid.getSelectionModel().deselect(T1LastRec, true, true);
                                if (T1Grid.getSelection().length == 0) {
                                    T1Grid.down('#T1commit').setDisabled(true);
                                    T1Grid.down('#T1delete').setDisabled(true);
                                    T1Grid.down('#T1in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "庫房別",
                dataIndex: 'WH_NO',
                width: 70,
                sortable: true
            }, {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 50,
                sortable: true
            }/*, {
                text: "進貨量",
                dataIndex: 'ORI_QTY',
                width: 60,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                style: 'text-align:left',
                format: '0'
            }*/, {
                header: "",
                flex: 1
            }],
            plugins: [
                Ext.create('Ext.grid.plugin.CellEditing', {
                    clicksToEdit: 1,
                    listeners: {
                        beforeedit: function (editor, context, eOpts) {
                            if (context.record.data.STATUS == 'Y' || context.record.data.STATUS == 'E') {
                                return false;
                            }
                            else {
                                return true;
                            }
                        }
                    }
                })
            ],
            listeners: {
                itemclick: function (self, record, item, index, e, eOpts) {
                    if (T1LastRec != null) {
                        rec = T1Store.getAt(T1Rec);
                        rec.set('PO_NO', "<span style='color:black'>" + T1Grid.getStore().data.items[T1Rec].data.PO_NO_REF + "</span>");
                        rec.commit();
                    }
                    T1Rec = index;
                    T1LastRec = record;
                    T1Grid.down('#T1add').setDisabled(false);
                    setFormT1a();
                    if (T1LastRec) {
                        msglabel("");
                    }
                },
                selectionchange: function (model, records) {
                    T1Grid.down('#T1add').setDisabled(false);       // 新增 固定隱藏
                    T1Grid.down('#T1commit').setDisabled(false);    // 儲存
                    T1Grid.down('#T1delete').setDisabled(false);    // 退貨
                    T1Grid.down('#T1in_pur').setDisabled(false);    // 進貨
                    
                    var length = 0;
                    length = records.length;
                    if (length == 1) {
                        return;
                    }
                    for (var i = 0; i < records.length; i++) {
                        
                        if (records[i].data.STATUS == 'N') { // 未進貨
                            T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                        }
                        if (records[i].data.STATUS == 'Y') { // 已進貨
                            T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                            T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                        }
                        if (records[i].data.STATUS == 'E') { // 已退貨
                            T1Grid.down('#T1commit').setDisabled(true);    // 儲存
                            T1Grid.down('#T1delete').setDisabled(true);    // 退貨
                            T1Grid.down('#T1in_pur').setDisabled(true);    // 進貨
                        }
                    }

                }
            }
        });

        // T1 點選master的項目後
        function setFormT1a() {
            if (T1LastRec) {
                isNew = false;
                rec = T1Store.getAt(T1Rec);
                rec.set('PO_NO', "<span style='color:blue'>" + T1Grid.getStore().data.items[T1Rec].data.PO_NO_REF + "</span>");
                rec.commit();
            }

            if (T1Grid.getSelection().length == 0) {
                T1Grid.down('#T1commit').setDisabled(true);
                T1Grid.down('#T1delete').setDisabled(true);
                T1Grid.down('#T1in_pur').setDisabled(true);
            }
            if (T1Grid.getSelection().length > 1) {
                return;
            }
            if (T1Grid.getStore().data.items[T1Rec].data.STATUS == 'Y') {
                T1Grid.down('#T1commit').setDisabled(true);
                T1Grid.down('#T1in_pur').setDisabled(true);
                T1Grid.down('#T1delete').setDisabled(false);
            }
            if (T1Grid.getStore().data.items[T1Rec].data.STATUS == 'N') {
                T1Grid.down('#T1commit').setDisabled(false);
                T1Grid.down('#T1in_pur').setDisabled(false);
                T1Grid.down('#T1delete').setDisabled(true);
            }
            if (T1Grid.getStore().data.items[T1Rec].data.STATUS == 'E') {
                T1Grid.down('#T1commit').setDisabled(true);
                T1Grid.down('#T1in_pur').setDisabled(true);
                T1Grid.down('#T1delete').setDisabled(true);
            }
        }

        // T1 點選退貨/進貨/儲存
        function T1Submit(s) {
            var selection = T1Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let MEMO = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_AMT = '';
            let PO_QTY = '';
            let PO_NO = '';
            let MMCODE = '';
            let SEQ = '';
            let VACCINE = '';
            let CONTRACNO = '';
            let E_PURTYPE = '';
            let PURDATE = '';
            let AGEN_NO = '';
            let PO_PRICE = '';
            let STATUS = '';
            let M_PURUN = '';
            let WH_NO = '';
            let M_DISCPERC = '';
            let UNIT_SWAP = '';
            let UPRICE = '';
            let DISC_CPRICE = '';
            let DISC_UPRICE = '';
            let TRANSKIND = '';
            let IFLAG = '';

            $.map(selection, function (item, key) {
                ACCOUNTDATE += Ext.util.Format.date(item.get('ACCOUNTDATE'), 'Xmd') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                MEMO += item.get('MEMO') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += Ext.util.Format.date(item.get('EXP_DATE'), 'Xmd') + ',';
                PO_AMT += item.get('PO_AMT') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                MMCODE += item.get('MMCODE') + ',';
                SEQ += item.get('SEQ') + ',';
                //----------------------------------
                VACCINE += item.get('VACCINE') + ',';
                CONTRACNO += item.get('CONTRACNO') + ',';
                E_PURTYPE += item.get('E_PURTYPE') + ',';
                PURDATE += item.get('PURDATE') + ',';
                AGEN_NO += item.get('AGEN_NO') + ',';
                PO_PRICE += item.get('PO_PRICE') + ',';
                STATUS += item.get('STATUS') + ',';
                M_PURUN += item.get('M_PURUN') + ',';
                WH_NO += item.get('WH_NO') + ',';
                M_DISCPERC += item.get('M_DISCPERC') + ',';
                UNIT_SWAP += item.get('UNIT_SWAP') + ',';
                UPRICE += item.get('UPRICE') + ',';
                DISC_CPRICE += item.get('DISC_CPRICE') + ',';
                DISC_UPRICE += item.get('DISC_UPRICE') + ',';
                TRANSKIND += item.get('TRANSKIND') + ',';
                IFLAG += item.get('IFLAG') + ',';
            })
            if ((s == 'B') &&
                ((STATUS.indexOf('T') >= 0) ||
                    (STATUS.indexOf('N') >= 0))) {
                Ext.Msg.alert('提醒', "勾選的資料中包含<span style='color:red'>尚未進貨</span>的項目，無法執行退貨，請檢核");
                msglabel("勾選的資料中包含<span style='color:red'>尚未進貨</span>的項目，無法執行退貨，請檢核");
            }
            else {
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();

                Ext.Ajax.request({
                    url: T1Set,
                    method: reqVal_p,
                    params: {
                        ACCOUNTDATE: ACCOUNTDATE,
                        DELI_QTY: DELI_QTY,
                        MEMO: MEMO,
                        LOT_NO: LOT_NO,
                        EXP_DATE: EXP_DATE,
                        PO_AMT: PO_AMT,
                        PO_QTY: PO_QTY,
                        PO_NO: PO_NO,
                        MMCODE: MMCODE,
                        SEQ: SEQ,
                        VACCINE: VACCINE,
                        CONTRACNO: CONTRACNO,
                        E_PURTYPE: E_PURTYPE,
                        PURDATE: PURDATE,
                        AGEN_NO: AGEN_NO,
                        PO_PRICE: PO_PRICE,
                        STATUS: STATUS,
                        M_PURUN: M_PURUN,
                        WH_NO: WH_NO,
                        M_DISCPERC: M_DISCPERC,
                        UNIT_SWAP: UNIT_SWAP,
                        UPRICE: UPRICE,
                        DISC_CPRICE: DISC_CPRICE,
                        DISC_UPRICE: DISC_UPRICE,
                        TRANSKIND: TRANSKIND,
                        IFLAG: IFLAG,
                        isTempSave :  s == 'C'? 'Y' :'N'
                    },
                    //async: true,
                    success: function (response) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            T1Grid.down('#T1commit').setDisabled(true);
                            T1Grid.down('#T1delete').setDisabled(true);
                            T1Grid.down('#T1in_pur').setDisabled(true);
                            switch (s) {
                                case "I":
                                    msglabel('點選的資料已新增');
                                    T1Grid.getSelectionModel().deselect(T1Rec, true, true);
                                    T1LastRec = T1Grid.getStore().data.items[T1Rec];
                                    T1Grid.getStore().data.items[T1Rec].data.PO_NO = "<span style='color:blue'>" + T1Grid.getStore().data.items[T1Rec].data.PO_NO_REF + "</span>";
                                    T1Grid.getStore().data.items[T1Rec].data.SEQ = data.msg.toString();
                                    break;
                                case "C":
                                    msglabel('點選的資料已儲存');
                                    T1LastRec = null;
                                    T1Load();
                                    break;
                                case "B":
                                    msglabel('點選的資料已執行退貨');
                                    T1LastRec = null;
                                    T1Load();
                                    break;
                                case "P":
                                    T1Set = '/api/CC0003/T1in_pur';
                                    T1in_pur();
                                    break;
                            }
                            myMask.hide();
                        }
                        else {
                            Ext.Msg.alert('失敗', data.msg);
                            msglabel(" " + data.msg);
                            T1Load();
                        }
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
                })
            }
        }

        // T1 新增一筆到畫面上
        function SetSubmitT1(s) {
            Temp = T1LastRec;
            T1Store.insert(T1Rec + 1, [
                {
                    VACCINE: T1LastRec.data.VACCINE,
                    CONTRACNO: T1LastRec.data.CONTRACNO,
                    E_PURTYPE: T1LastRec.data.E_PURTYPE,
                    PURDATE: T1LastRec.data.PURDATE,
                    PO_NO: T1LastRec.data.PO_NO,
                    PO_NO_REF: T1LastRec.data.PO_NO_REF,
                    AGEN_NO: T1LastRec.data.AGEN_NO,
                    AGEN_NO_NAME: T1LastRec.data.AGEN_NO_NAME,
                    MMCODE: T1LastRec.data.MMCODE,
                    MMNAME_C: T1LastRec.data.MMNAME_C,
                    MMNAME_E: T1LastRec.data.MMNAME_E,
                    ACCOUNTDATE: '',
                    ACCOUNTDATE_REF: '',
                    PO_QTY: T1LastRec.data.PO_QTY,
                    DELI_QTY: 0,
                    DELI_QTY_REF: 0,
                    INFLAG: '',
                    PO_PRICE: T1LastRec.data.PO_PRICE,
                    PO_AMT: T1LastRec.data.PO_AMT,
                    M_PURUN: T1LastRec.data.M_PURUN,
                    LOT_NO: '',
                    LOT_NO_REF: '',
                    EXP_DATE: '',
                    EXP_DATE_REF: '',
                    MEMO: '',
                    MEMO_REF: '',
                    WH_NO: T1LastRec.data.WH_NO,
                    STATUS: 'N',
                    ORI_QTY: T1LastRec.data.ORI_QTY,
                    M_DISCPERC: T1LastRec.data.M_DISCPERC,
                    UNIT_SWAP: T1LastRec.data.UNIT_SWAP,
                    UPRICE: T1LastRec.data.UPRICE,
                    DISC_CPRICE: T1LastRec.data.DISC_CPRICE,
                    DISC_UPRICE: T1LastRec.data.DISC_UPRICE,
                    TRANSKIND: T1LastRec.data.TRANSKIND,
                    IFLAG: T1LastRec.data.IFLAG
                }]);
            T1Grid.getStore().data.items[T1Rec].data.PO_NO = "<span style='color:black'>" + T1Grid.getStore().data.items[T1Rec].data.PO_NO_REF + "</span>";
            T1Grid.getView().refresh();
            T1Rec = T1Rec + 1;
            T1LastRec = T1Grid.getStore().data.items[T1Rec];
            T1Grid.getSelectionModel().select(T1Rec, true, true);
            T1AddSubmit();
        }

        // T1 新增資料到資料庫中
        function T1AddSubmit() {
            var ACCOUNTDATE = Temp.data.ACCOUNTDATE;
            var DELI_QTY = Temp.data.DELI_QTY;
            var MEMO = Temp.data.MEMO;
            var LOT_NO = Temp.data.LOT_NO;
            var EXP_DATE = Temp.data.EXP_DATE;
            var PO_AMT = Temp.data.PO_AMT;
            var PO_QTY = Temp.data.PO_QTY;
            var PO_NO = Temp.data.PO_NO_REF;
            var MMCODE = Temp.data.MMCODE;
            var SEQ = Temp.data.SEQ;
            var VACCINE = Temp.data.VACCINE;
            var CONTRACNO = Temp.data.CONTRACNO;
            var E_PURTYPE = Temp.data.E_PURTYPE;
            var PURDATE = Temp.data.PURDATE;
            var AGEN_NO = Temp.data.AGEN_NO;
            var PO_PRICE = Temp.data.PO_PRICE;
            var STATUS = Temp.data.STATUS;
            var M_PURUN = Temp.data.M_PURUN;
            var WH_NO = Temp.data.WH_NO;
            var M_DISCPERC = Temp.data.M_DISCPERC;
            var UNIT_SWAP = Temp.data.UNIT_SWAP;
            var UPRICE = Temp.data.UPRICE;
            var DISC_CPRICE = Temp.data.DISC_CPRICE;
            var DISC_UPRICE = Temp.data.DISC_UPRICE;
            var TRANSKIND = Temp.data.TRANSKIND;
            var IFLAG = Temp.data.IFLAG;

            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                params: {
                    ACCOUNTDATE: ACCOUNTDATE,
                    DELI_QTY: DELI_QTY,
                    MEMO: MEMO,
                    LOT_NO: LOT_NO,
                    EXP_DATE: EXP_DATE,
                    PO_AMT: PO_AMT,
                    PO_QTY: PO_QTY,
                    PO_NO: PO_NO,
                    MMCODE: MMCODE,
                    SEQ: SEQ,
                    VACCINE: VACCINE,
                    CONTRACNO: CONTRACNO,
                    E_PURTYPE: E_PURTYPE,
                    PURDATE: PURDATE,
                    AGEN_NO: AGEN_NO,
                    PO_PRICE: PO_PRICE,
                    STATUS: STATUS,
                    M_PURUN: M_PURUN,
                    WH_NO: WH_NO,
                    M_DISCPERC: M_DISCPERC,
                    UNIT_SWAP: UNIT_SWAP,
                    UPRICE: UPRICE,
                    DISC_CPRICE: DISC_CPRICE,
                    DISC_UPRICE: DISC_UPRICE,
                    TRANSKIND: TRANSKIND,
                    IFLAG: IFLAG
                },
                //async: true,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        msglabel('點選的資料已新增');
                        T1Grid.getSelectionModel().deselect(T1Rec - 1, true, true);
                        T1LastRec = T1Grid.getStore().data.items[T1Rec];
                        T1Grid.getStore().data.items[T1Rec].data.PO_NO = "<span style='color:blue'>" + T1Grid.getStore().data.items[T1Rec].data.PO_NO_REF + "</span>";
                        T1Grid.getStore().data.items[T1Rec].data.SEQ = data.msg.toString();
                        myMask.hide();
                        T1Grid.down('#T1commit').setDisabled(true);
                        T1Grid.down('#T1delete').setDisabled(true);
                        T1Grid.down('#T1in_pur').setDisabled(true);
                    }
                    else {
                        myMask.hide();
                        Ext.Msg.alert('失敗', data.msg);
                        msglabel(" " + data.msg);
                        T1Load();
                    }
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
            })
        }

        // T1 檢核進貨量
        function T1CheckQty(s) {
            var PO_NO_TEMP = '';
            var MMCODE_TEMP = '';
            var PO_QTY_TEMP = 0;
            var DELI_QTY_TEMP = 0;
            var CHECK_FLAG = 'Y';
            var CHECK_MSG = '';
            
            for (var i = 0; i < T1Grid.getStore().data.items.length; i++) {
                if (PO_NO_TEMP == '' && T1Grid.getStore().data.items[i].data.OUTFLAG != '退貨') {
                    PO_NO_TEMP = T1Grid.getStore().data.items[i].data.PO_NO_REF;
                    MMCODE_TEMP = T1Grid.getStore().data.items[i].data.MMCODE;
                    PO_QTY_TEMP = T1Grid.getStore().data.items[i].data.PO_QTY;
                }
                if ((PO_NO_TEMP == T1Grid.getStore().data.items[i].data.PO_NO_REF) &&
                    (MMCODE_TEMP == T1Grid.getStore().data.items[i].data.MMCODE) &&
                    (T1Grid.getStore().data.items[i].data.OUTFLAG != '退貨')) {
                    DELI_QTY_TEMP = parseInt(DELI_QTY_TEMP) + parseInt(T1Grid.getStore().data.items[i].data.DELI_QTY);
                }
                else if (T1Grid.getStore().data.items[i].data.OUTFLAG != '退貨') {
                    if (DELI_QTY_TEMP > PO_QTY_TEMP) {
                        CHECK_FLAG = 'N';
                        CHECK_MSG += "採購單號" + PO_NO_TEMP + " ,院內碼" + MMCODE_TEMP + ' 的累計進貨量' + DELI_QTY_TEMP + "<span style='color:red'>不可大於</span>進貨量" + PO_QTY_TEMP + "<br>";
                    }
                    PO_NO_TEMP = T1Grid.getStore().data.items[i].data.PO_NO_REF;
                    MMCODE_TEMP = T1Grid.getStore().data.items[i].data.MMCODE;
                    PO_QTY_TEMP = T1Grid.getStore().data.items[i].data.PO_QTY;
                    DELI_QTY_TEMP = Number(T1Grid.getStore().data.items[i].data.DELI_QTY);
                }
            }

            
            var selection = T1Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_QTY = '';
            let PO_NO = '';
            let SEQ = '';
            let STATUS = '';
            let MMCODE = '';
            $.map(selection, function (item, key) {
                ACCOUNTDATE += item.get('ACCOUNTDATE') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += item.get('EXP_DATE') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                SEQ += item.get('SEQ') + ',';
                STATUS += item.get('STATUS') + ',';
                MMCODE += item.get('MMCODE') + ',';
            })

            var null_flag = 'Y';
            var qty_flag = 'Y';
            if (s == 'P') {
                //檢查是否有進貨日期/批號/效期未填
                var ACCOUNTDATES = ACCOUNTDATE.split(',');
                var LOT_NOS = LOT_NO.split(',');
                var EXP_DATES = EXP_DATE.split(',');
                var DELI_QTYS = DELI_QTY.split(',');
                var PO_QTYS = PO_QTY.split(',');
                var PO_NOS = PO_NO.split(',');
                var SEQS = SEQ.split(',');
                var MMCODES = MMCODE.split(',');
                
                for (var i = 0; i < MMCODES.length - 1; i++) {
                    if (LOT_NOS[i] == '' || EXP_DATES[i] == '') {
                        null_flag = 'N';
                        Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        msglabel("勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        break;
                    }

                    if (parseInt(DELI_QTYS[i]) < parseInt(PO_QTYS[i])) {
                        qty_flag = 'N';
                    }
                }
            }

            if ((null_flag == 'Y') && (qty_flag == 'N')) {
                Ext.MessageBox.confirm('提醒', "勾選的資料中有<span style='color:red'>進貨量 < 採購量</span>，是否確定執行進貨？", function (btn, text) {
                    if (btn === 'yes') {
                        if ((CHECK_FLAG == 'Y') && (s == 'C')) {
                            T1Submit('C');
                        }
                        else if ((CHECK_FLAG == 'Y') && (s == 'P')) {
                            T1Submit('P');
                        }
                        else {
                            Ext.Msg.alert("提醒", CHECK_MSG);
                            msglabel(CHECK_MSG);
                        }
                    }
                });
            }
            else if ((null_flag == 'Y') && (qty_flag == 'Y')) {
                if ((CHECK_FLAG == 'Y') && (s == 'C')) {
                    T1Submit('C');
                }
                else if ((CHECK_FLAG == 'Y') && (s == 'P')) {
                    T1Submit('P');
                }
                else {
                    Ext.Msg.alert("提醒", CHECK_MSG);
                    msglabel(CHECK_MSG);
                }
            }

            
        }

        // T1 進貨檢查
        function T1in_pur() {
            var selection = T1Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_QTY = '';
            let PO_NO = '';
            let SEQ = '';
            let STATUS = '';
            let MMCODE = '';
            $.map(selection, function (item, key) {
                ACCOUNTDATE += item.get('ACCOUNTDATE') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += item.get('EXP_DATE') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                SEQ += item.get('SEQ') + ',';
                STATUS += item.get('STATUS') + ',';
                MMCODE += item.get('MMCODE') + ',';
            })
            
            if (STATUS.indexOf('Y') >= 0 || STATUS.indexOf('E') >= 0) {
                Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>已進貨</span>或<span style='color:red'>已退貨</span>的資料，無法執行進貨，請檢核");
                msglabel("勾選的資料中有<span style='color:red'>已進貨</span>或<span style='color:red'>已退貨</span>的資料，無法執行進貨，請檢核");
                T1Grid.down('#T1commit').setDisabled(false);
                T1Grid.down('#T1delete').setDisabled(false);
                T1Grid.down('#T1in_pur').setDisabled(false);
            }
            else {
                //檢查是否有進貨日期/批號/效期未填
                var ACCOUNTDATES = ACCOUNTDATE.split(',');
                var LOT_NOS = LOT_NO.split(',');
                var EXP_DATES = EXP_DATE.split(',');
                var DELI_QTYS = DELI_QTY.split(',');
                var PO_QTYS = PO_QTY.split(',');
                var PO_NOS = PO_NO.split(',');
                var SEQS = SEQ.split(',');
                var MMCODES = MMCODE.split(',');
                var null_flag = 'Y';
                var qty_flag = 'Y';
                for (var i = 0; i < MMCODES.length - 1; i++) {
                    if (LOT_NOS[i] == '' || EXP_DATES[i] == '') {
                        null_flag = 'N';
                        Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        msglabel("勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        //T1Grid.down('#T1commit').setDisabled(false);
                        //T1Grid.down('#T1delete').setDisabled(false);
                        //T1Grid.down('#T1in_pur').setDisabled(false);



                        break;
                    }
                    if (parseInt(DELI_QTYS[i]) < parseInt(PO_QTYS[i])) {
                        qty_flag = 'N';
                    }
                }
                if ((null_flag == 'Y') && (qty_flag == 'N')) {
                    Ext.MessageBox.confirm('提醒', "勾選的資料中有<span style='color:red'>進貨量 < 採購量</span>，是否確定執行進貨？", function (btn, text) {
                        if (btn === 'yes') {
                            T1in_pur_submit(PO_NO, SEQ, MMCODE);
                        }
                    });
                }
                else if ((null_flag == 'Y') && (qty_flag == 'Y')) {
                    T1in_pur_submit(PO_NO, SEQ, MMCODE);
                }
            }
        }

        // T1 確認進貨
        function T1in_pur_submit(PO_NO, SEQ, MMCODE) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                params: {
                    PO_NO: PO_NO,
                    SEQ: SEQ,
                    MMCODE: MMCODE
                },
                //async: true,
                success: function (response) {
                    myMask.hide();
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        if (data.msg.toString() == "") {
                            msglabel('點選的資料已執行進貨');
                            T1LastRec = null;
                            T1Load();
                            myMask.hide();
                            T1Grid.down('#T1commit').setDisabled(true);
                            T1Grid.down('#T1delete').setDisabled(true);
                            T1Grid.down('#T1in_pur').setDisabled(true);
                        } else {
                            Ext.MessageBox.alert('警示', data.msg);
                        }
                    }
                    else {
                        Ext.Msg.alert('失敗', data.msg);
                        msglabel(" " + data.msg);
                        T1Load();
                    }
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
            })
        }
    }
    //#endregion
    //#region T2
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T2 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T2 查詢欄位
        var T2QueryForm = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true,
            defaultType: 'textfield',
            fieldDefaults: {
                labelAlign: 'right',
                msgTarget: 'side',
                labelWidth: 70,
                width: 180
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫別',
                        name: 'P4',
                        id: 'P4',
                        labelWidth: 50,
                        width: 200,
                        store: WH_NO_Store,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        fieldCls: 'required',
                        allowBlank: false, // 欄位是否為必填
                        blankText: "請輸入庫別",
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '廠商代碼',
                        name: 'P5',
                        id: 'P5',
                        labelWidth: 70,
                        width: 220,
                        store: AGEN_NO_Store,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        allowBlank: true, // 欄位是否為必填
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '採購日期',
                        name: 'P6',
                        id: 'P6',
                        labelWidth: 70,
                        width: 170,
                        value: new Date(),
                        enforceMaxLength: true,
                        fieldCls: 'required',
                        allowBlank: false, // 欄位是否為必填
                        blankText: "請輸入採購日期",
                        renderer: function (value, meta, record) {
                            return Ext.util.Format.date(value, 'X/m/d');
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'query',
                        text: '查詢',
                        handler: function () {
                            T2Load();
                            msglabel("");
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'clean',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            T2QueryForm.getForm().findField("P0").setValue(first_wh_no);
                            T2QueryForm.getForm().findField("P1").setValue('');
                            T2QueryForm.getForm().findField("P2").setValue(Ext.Date.add(new Date(), Ext.Date.DAY, -3));
                            T2QueryForm.getForm().findField("P2_1").setValue(new Date());
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }]
            }]
        });

        // T2 Store
        var T2Store = Ext.create('WEBAPP.store.CC0003_2', { // 定義於/Scripts/app/store/CC0003_2.js
            listeners: {
                beforeload: function (store, options) {
                    store.removeAll();
                    // 載入前將查詢條件P0~P2的值代入參數
                    var np = {
                        p0: T2QueryForm.getForm().findField('P4').getValue(),
                        p1: T2QueryForm.getForm().findField('P5').getValue(),
                        p2: T2QueryForm.getForm().findField('P6').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        // T2 toolbar,包含換頁、新增/修改/刪除鈕
        var T2Tool = Ext.create('Ext.PagingToolbar', {
            store: T2Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    itemId: 'T2add', text: '新增', disabled: true, hidden: true, handler: function () {
                        T2Set = '../../../api/CC0003/Insert';
                        SetSubmitT2('I');
                    }
                },
                {
                    itemId: 'T2commit', text: '儲存', disabled: true, handler: function () {
                        T2Set = '../../../api/CC0003/Commit';
                        T2CheckQty('C');
                    }
                },
                {
                    itemId: 'T2delete', text: '退貨', disabled: true, hidden: true,
                    handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('退貨', '是否確定執行退貨？', function (btn, text) {
                            if (btn === 'yes') {
                                T2Set = '/api/CC0003/Back';
                                T2Submit('B');
                            }
                        });
                    }
                },
                {
                    itemId: 'T2in_pur', text: '進貨', disabled: true,
                    handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('提醒', "是否確定執行進貨,增加庫存量？", function (btn, text) {
                            if (btn === 'yes') {
                                T2Set = '/api/CC0003/Commit';
                                T2CheckQty('P');
                                //T2in_pur();
                            }
                        });
                    }
                }]
        });

        // T2 查詢結果資料列表
        var T2Grid = Ext.create('Ext.grid.Panel', {
            //title: '',
            store: T2Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T2',
            dockedItems: [
                {
                    items: [T2QueryForm]
                }, {
                    dock: 'top',
                    xtype: 'toolbar',
                    items: [T2Tool]
                }],
            selModel: Ext.create('Ext.selection.CheckboxModel', {
                checkOnly: true,
                injectCheckbox: 'first',
                mode: 'SIMPLE',
                showHeaderCheckbox: true
            }),
            columns: [{
                dataIndex: 'ACCOUNTDATE_REF',
                text: "原進貨日期",
                hidden: true
            }, {
                dataIndex: 'DELI_QTY_REF,',
                text: "原進貨量",
                hidden: true
            }, {
                dataIndex: 'LOT_NO_REF',
                text: "原批號",
                hidden: true
            }, {
                dataIndex: 'EXP_DATE_REF',
                text: "原效期",
                hidden: true
            }, {
                dataIndex: 'MEMO_REF',
                text: "原備註",
                hidden: true
            }, {
                dataIndex: 'SEQ',
                text: "流水號",
                hidden: true
            }, {
                dataIndex: 'PO_NO_REF',
                text: "採購單號",
                hidden: true
            }, {
                dataIndex: 'M_DISCPERC',
                text: "折讓比",
                hidden: true
            }, {
                dataIndex: 'UNIT_SWAP',
                text: "轉換率",
                hidden: true
            }, {
                dataIndex: 'UPRICE',
                text: "最小單價",
                hidden: true
            }, {
                dataIndex: 'DISC_CPRICE',
                text: "優惠合約單價",
                hidden: true
            }, {
                dataIndex: 'DISC_UPRICE',
                text: "優惠最小單價",
                hidden: true
            }, {
                dataIndex: 'TRANSKIND',
                text: "異動類別",
                hidden: true
            }, {
                dataIndex: 'IFLAG',
                text: "新增識別",
                hidden: true
            }, {
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            }, {
                text: "疫苗",
                dataIndex: 'VACCINE',
                width: 40,
                sortable: true
            }, {
                text: "合約碼",
                dataIndex: 'CONTRACNO',
                width: 50,
                sortable: true
            }, {
                text: "案別",
                dataIndex: 'E_PURTYPE',
                width: 40,
                sortable: true
            }, {
                text: "採購日期",
                dataIndex: 'PURDATE',
                width: 70,
                sortable: true
            }, {
                text: "採購單號",
                dataIndex: 'PO_NO',
                width: 90,
                sortable: true
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO_NAME',
                width: 150,
                sortable: true
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70,
                sortable: true
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150,
                sortable: true
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200,
                sortable: true
            }, {
                xtype: 'datecolumn',
                text: "進貨日期",
                dataIndex: 'ACCOUNTDATE',
                width: 90,
                sortable: true,
                format: 'Xmd',
                editor: {
                    xtype: 'datefield',
                    allowBlank: false,
                    format: 'Xmd',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if ((Ext.util.Format.date(newVal, 'Xmd') != T2LastRec.data.ACCOUNTDATE_REF) ||
                                T2LastRec.data.DELI_QTY != T2LastRec.data.DELI_QTY_REF ||
                                T2LastRec.data.MEMO != T2LastRec.data.MEMO_REF ||
                                T2LastRec.data.LOT_NO != T2LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T2LastRec.data.EXP_DATE, 'Xmd') != T2LastRec.data.EXP_DATE_REF) {
                                T2Grid.getSelectionModel().select(T2LastRec, true, true);
                                T2Grid.down('#T2commit').setDisabled(false);
                                T2Grid.down('#T2delete').setDisabled(false);
                                T2Grid.down('#T2in_pur').setDisabled(false);
                            }
                            else {
                                T2Grid.getSelectionModel().deselect(T2LastRec, true, true);
                                if (T2Grid.getSelection().length == 0) {
                                    T2Grid.down('#T2commit').setDisabled(true);
                                    T2Grid.down('#T2delete').setDisabled(true);
                                    T2Grid.down('#T2in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "採購量",
                dataIndex: 'PO_QTY',
                width: 90,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                style: 'text-align:left',
                format: '0'
            }, {
                text: "<span style='color:red'>進貨量</span>",
                dataIndex: 'DELI_QTY',
                width: 90,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                minValue: 0,
                allowBlank: false,
                style: 'text-align:left',
                format: '0',
                editor: {
                    xtype: 'numberfield',
                    hideTrigger: true,
                    allowBlank: false,
                    listeners: {
                        blur: function (field, event, eOpts) {
                            if (field.value == null) {
                                rec2 = T2Store.getAt(T2Rec);
                                rec2.set('DELI_QTY', 0);
                                rec2.set('PO_AMT', 0);
                                rec2.commit();
                                msglabel("進貨量<span style='color:red'>不可為空</span>");
                            }
                        },
                        change: function (field, newVal, oldVal) {
                            if (newVal > T2LastRec.data.PO_QTY) {
                                T2Grid.getView().refresh();
                                msglabel("進貨量<span style='color:red'>不可</span>超過採購量");
                            }
                            else {
                                if (((newVal != T2LastRec.data.DELI_QTY_REF) && (newVal <= T2LastRec.data.PO_QTY)) ||
                                    Ext.util.Format.date(T2LastRec.data.ACCOUNTDATE, 'Xmd') != T2LastRec.data.ACCOUNTDATE_REF ||
                                    T2LastRec.data.MEMO != T2LastRec.data.MEMO_REF ||
                                    T2LastRec.data.LOT_NO != T2LastRec.data.LOT_NO_REF ||
                                    Ext.util.Format.date(T2LastRec.data.EXP_DATE, 'Xmd') != T2LastRec.data.EXP_DATE_REF) {
                                    T2Grid.getSelectionModel().select(T2LastRec, true, true);
                                    T2Grid.down('#T2commit').setDisabled(false);
                                    T2Grid.down('#T2delete').setDisabled(false);
                                    T2Grid.down('#T2in_pur').setDisabled(false);
                                    rec2 = T2Store.getAt(T2Rec);
                                    rec2.set('PO_AMT', T2LastRec.data.PO_PRICE * newVal);
                                    rec2.set('DELI_QTY', newVal);
                                    rec2.commit();
                                }
                                else {
                                    T2Grid.getSelectionModel().deselect(T2LastRec, true, true);
                                    if (T2Grid.getSelection().length == 0) {
                                        T2Grid.down('#T2commit').setDisabled(true);
                                        T2Grid.down('#T2delete').setDisabled(true);
                                        T2Grid.down('#T2in_pur').setDisabled(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }, {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 70,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                style: 'text-align:left',
                format: '0.00'
            }, {
                text: "總金額",
                dataIndex: 'PO_AMT',
                width: 80,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                style: 'text-align:left',
                format: '0.00'
            }, {
                text: "進貨單位",
                dataIndex: 'M_PURUN',
                width: 70,
                sortable: true
            }, {
                text: "<span style='color:red'>批號</span>",
                dataIndex: 'LOT_NO',
                width: 120,
                sortable: true,
                editor: {
                    xtype: 'textfield',
                    maxLength: 20,
                    //hideTrigger: true,
                    //allowBlank: false,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (newVal != T2LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T2LastRec.data.ACCOUNTDATE, 'Xmd') != T2LastRec.data.ACCOUNTDATE_REF ||
                                T2LastRec.data.MEMO != T2LastRec.data.MEMO_REF ||
                                T2LastRec.data.DELI_QTY != T2LastRec.data.DELI_QTY_REF ||
                                Ext.util.Format.date(T2LastRec.data.EXP_DATE, 'Xmd') != T2LastRec.data.EXP_DATE_REF) {
                                T2Grid.getSelectionModel().select(T2LastRec, true, true);
                                T2Grid.down('#T2commit').setDisabled(false);
                                T2Grid.down('#T2delete').setDisabled(false);
                                T2Grid.down('#T2in_pur').setDisabled(false);
                            }
                            else {
                                T2Grid.getSelectionModel().deselect(T2LastRec, true, true);
                                if (T2Grid.getSelection().length == 0) {
                                    T2Grid.down('#T2commit').setDisabled(true);
                                    T2Grid.down('#T2delete').setDisabled(true);
                                    T2Grid.down('#T2in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                xtype: 'datecolumn',
                text: "<span style='color:red'>效期</span>",
                dataIndex: 'EXP_DATE',
                width: 100,
                sortable: true,
                format: 'Xmd',
                editor: {
                    xtype: 'datefield',
                    allowBlank: false,
                    format: 'Xmd',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if ((Ext.util.Format.date(newVal, 'Xmd') != T2LastRec.data.EXP_DATE_REF) ||
                                T2LastRec.data.DELI_QTY != T2LastRec.data.DELI_QTY_REF ||
                                T2LastRec.data.MEMO != T2LastRec.data.MEMO_REF ||
                                T2LastRec.data.LOT_NO != T2LastRec.data.LOT_NO_REF ||
                                Ext.util.Format.date(T2LastRec.data.ACCOUNTDATE, 'Xmd') != T2LastRec.data.ACCOUNTDATE_REF) {
                                T2Grid.getSelectionModel().select(T2LastRec, true, true);
                                T2Grid.down('#T2commit').setDisabled(false);
                                T2Grid.down('#T2delete').setDisabled(false);
                                T2Grid.down('#T2in_pur').setDisabled(false);
                            }
                            else {
                                T2Grid.getSelectionModel().deselect(T2LastRec, true, true);
                                if (T2Grid.getSelection().length == 0) {
                                    T2Grid.down('#T2commit').setDisabled(true);
                                    T2Grid.down('#T2delete').setDisabled(true);
                                    T2Grid.down('#T2in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "<span style='color:red'>備註</span>",
                dataIndex: 'MEMO',
                width: 120,
                sortable: true,
                editor: {
                    xtype: 'textfield',
                    hideTrigger: true,
                    //allowBlank: false,
                    maxLength: 300,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (newVal != T2LastRec.data.MEMO_REF ||
                                Ext.util.Format.date(T2LastRec.data.ACCOUNTDATE, 'Xmd') != T2LastRec.data.ACCOUNTDATE_REF ||
                                T2LastRec.data.LOT_NO != T2LastRec.data.LOT_NO_REF ||
                                T2LastRec.data.DELI_QTY != T2LastRec.data.DELI_QTY_REF ||
                                Ext.util.Format.date(T2LastRec.data.EXP_DATE, 'Xmd') != T2LastRec.data.EXP_DATE_REF) {
                                T2Grid.getSelectionModel().select(T2LastRec, true, true);
                                T2Grid.down('#T2commit').setDisabled(false);
                                T2Grid.down('#T2delete').setDisabled(false);
                                T2Grid.down('#T2in_pur').setDisabled(false);
                            }
                            else {
                                T2Grid.getSelectionModel().deselect(T2LastRec, true, true);
                                if (T2Grid.getSelection().length == 0) {
                                    T2Grid.down('#T2commit').setDisabled(true);
                                    T2Grid.down('#T2delete').setDisabled(true);
                                    T2Grid.down('#T2in_pur').setDisabled(true);
                                }
                            }
                        }
                    }
                }
            }, {
                text: "庫房別",
                dataIndex: 'WH_NO',
                width: 70,
                sortable: true
            }, {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 50,
                sortable: true
            }, {
                header: "",
                flex: 1
            }],
            plugins: [
                Ext.create('Ext.grid.plugin.CellEditing', {
                    clicksToEdit: 1,
                    listeners: {
                        beforeedit: function (editor, context, eOpts) {
                            if (context.record.data.STATUS == 'Y') {
                                return false;
                            }
                            else {
                                return true;
                            }
                        }
                    }
                })
            ],
            listeners: {
                itemclick: function (self, record, item, index, e, eOpts) {
                    if (T2LastRec != null) {
                        rec2 = T2Store.getAt(T2Rec);
                        rec2.set('PO_NO', "<span style='color:black'>" + T2Grid.getStore().data.items[T2Rec].data.PO_NO_REF + "</span>");
                        rec2.commit();
                    }
                    T2Rec = index;
                    T2LastRec = record;
                    T2Grid.down('#T2add').setDisabled(false);
                    setFormT2a();
                    if (T2LastRec) {
                        msglabel("");
                    }
                }
            }
        });

        // T2 點選master的項目後
        function setFormT2a() {
            if (T2LastRec) {
                isNew = false;
                rec2 = T2Store.getAt(T2Rec);
                rec2.set('PO_NO', "<span style='color:blue'>" + T2Grid.getStore().data.items[T2Rec].data.PO_NO_REF + "</span>");
                rec2.commit();
            }

            if (T2Grid.getSelection().length == 0) {
                T2Grid.down('#T2commit').setDisabled(true);
                T2Grid.down('#T2delete').setDisabled(true);
                T2Grid.down('#T2in_pur').setDisabled(true);
            }
            else {
                T2Grid.down('#T2commit').setDisabled(false);
                T2Grid.down('#T2delete').setDisabled(false);
                T2Grid.down('#T2in_pur').setDisabled(false);
            }
        }

        // T2 點選退貨/進貨/儲存
        function T2Submit(s) {
            var selection = T2Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let MEMO = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_AMT = '';
            let PO_QTY = '';
            let PO_NO = '';
            let MMCODE = '';
            let SEQ = '';
            let VACCINE = '';
            let CONTRACNO = '';
            let E_PURTYPE = '';
            let PURDATE = '';
            let AGEN_NO = '';
            let PO_PRICE = '';
            let STATUS = '';
            let M_PURUN = '';
            let WH_NO = '';
            let M_DISCPERC = '';
            let UNIT_SWAP = '';
            let UPRICE = '';
            let DISC_CPRICE = '';
            let DISC_UPRICE = '';
            let TRANSKIND = '';
            let IFLAG = '';

            $.map(selection, function (item, key) {
                ACCOUNTDATE += Ext.util.Format.date(item.get('ACCOUNTDATE'), 'Xmd') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                MEMO += item.get('MEMO') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += Ext.util.Format.date(item.get('EXP_DATE'), 'Xmd') + ',';
                PO_AMT += item.get('PO_AMT') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                MMCODE += item.get('MMCODE') + ',';
                SEQ += item.get('SEQ') + ',';
                //----------------------------------
                VACCINE += item.get('VACCINE') + ',';
                CONTRACNO += item.get('CONTRACNO') + ',';
                E_PURTYPE += item.get('E_PURTYPE') + ',';
                PURDATE += item.get('PURDATE') + ',';
                AGEN_NO += item.get('AGEN_NO') + ',';
                PO_PRICE += item.get('PO_PRICE') + ',';
                STATUS += item.get('STATUS') + ',';
                M_PURUN += item.get('M_PURUN') + ',';
                WH_NO += item.get('WH_NO') + ',';
                M_DISCPERC += item.get('M_DISCPERC') + ',';
                UNIT_SWAP += item.get('UNIT_SWAP') + ',';
                UPRICE += item.get('UPRICE') + ',';
                DISC_CPRICE += item.get('DISC_CPRICE') + ',';
                DISC_UPRICE += item.get('DISC_UPRICE') + ',';
                TRANSKIND += item.get('TRANSKIND') + ',';
                IFLAG += item.get('IFLAG') + ',';
            })
            if ((s == 'B') &&
                ((STATUS.indexOf('T') >= 0) ||
                    (STATUS.indexOf('N') >= 0))) {
                Ext.Msg.alert('提醒', "勾選的資料中包含<span style='color:red'>尚未進貨</span>的項目，無法執行退貨，請檢核");
                msglabel("勾選的資料中包含<span style='color:red'>尚未進貨</span>的項目，無法執行退貨，請檢核");
            }
            else {
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();

                Ext.Ajax.request({
                    url: T2Set,
                    method: reqVal_p,
                    params: {
                        ACCOUNTDATE: ACCOUNTDATE,
                        DELI_QTY: DELI_QTY,
                        MEMO: MEMO,
                        LOT_NO: LOT_NO,
                        EXP_DATE: EXP_DATE,
                        PO_AMT: PO_AMT,
                        PO_QTY: PO_QTY,
                        PO_NO: PO_NO,
                        MMCODE: MMCODE,
                        SEQ: SEQ,
                        VACCINE: VACCINE,
                        CONTRACNO: CONTRACNO,
                        E_PURTYPE: E_PURTYPE,
                        PURDATE: PURDATE,
                        AGEN_NO: AGEN_NO,
                        PO_PRICE: PO_PRICE,
                        STATUS: STATUS,
                        M_PURUN: M_PURUN,
                        WH_NO: WH_NO,
                        M_DISCPERC: M_DISCPERC,
                        UNIT_SWAP: UNIT_SWAP,
                        UPRICE: UPRICE,
                        DISC_CPRICE: DISC_CPRICE,
                        DISC_UPRICE: DISC_UPRICE,
                        TRANSKIND: TRANSKIND,
                        IFLAG: IFLAG
                    },
                    //async: true,
                    success: function (response) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        
                        if (data.success) {
                            T2Grid.down('#T2commit').setDisabled(true);
                            T2Grid.down('#T2delete').setDisabled(true);
                            T2Grid.down('#T2in_pur').setDisabled(true);
                            switch (s) {
                                case "I":
                                    msglabel('點選的資料已新增');
                                    T2Grid.getSelectionModel().deselect(T2Rec, true, true);
                                    T2LastRec = T2Grid.getStore().data.items[T2Rec];
                                    T2Grid.getStore().data.items[T2Rec].data.PO_NO = "<span style='color:blue'>" + T2Grid.getStore().data.items[T2Rec].data.PO_NO_REF + "</span>";
                                    T2Grid.getStore().data.items[T2Rec].data.SEQ = data.msg.toString();
                                    break;
                                case "C":
                                    msglabel('點選的資料已儲存');
                                    T2LastRec = null;
                                    T2Load();
                                    break;
                                case "B":
                                    msglabel('點選的資料已執行退貨');
                                    T2LastRec = null;
                                    T2Load();
                                    break;
                                case "P":
                                    T2Set = '/api/CC0003/T2in_pur';
                                    T2in_pur();
                                    break;
                            }
                            myMask.hide();
                        }
                        else {
                            Ext.Msg.alert('失敗', data.msg);
                            msglabel(" " + data.msg);
                            T2Load();
                        }
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
                })
            }
        }

        // T2 新增一筆到畫面上
        function SetSubmitT2(s) {
            Temp = T2LastRec;
            T2Store.insert(T2Rec + 1, [
                {
                    VACCINE: T2LastRec.data.VACCINE,
                    CONTRACNO: T2LastRec.data.CONTRACNO,
                    E_PURTYPE: T2LastRec.data.E_PURTYPE,
                    PURDATE: T2LastRec.data.PURDATE,
                    PO_NO: T2LastRec.data.PO_NO,
                    PO_NO_REF: T2LastRec.data.PO_NO_REF,
                    AGEN_NO: T2LastRec.data.AGEN_NO,
                    AGEN_NO_NAME: T2LastRec.data.AGEN_NO_NAME,
                    MMCODE: T2LastRec.data.MMCODE,
                    MMNAME_C: T2LastRec.data.MMNAME_C,
                    MMNAME_E: T2LastRec.data.MMNAME_E,
                    ACCOUNTDATE: '',
                    ACCOUNTDATE_REF: '',
                    PO_QTY: T2LastRec.data.PO_QTY,
                    DELI_QTY: 0,
                    DELI_QTY_REF: 0,
                    PO_PRICE: T2LastRec.data.PO_PRICE,
                    PO_AMT: T2LastRec.data.PO_AMT,
                    M_PURUN: T2LastRec.data.M_PURUN,
                    LOT_NO: '',
                    LOT_NO_REF: '',
                    EXP_DATE: '',
                    EXP_DATE_REF: '',
                    MEMO: '',
                    MEMO_REF: '',
                    WH_NO: T2LastRec.data.WH_NO,
                    STATUS: 'N',
                    ORI_QTY: T2LastRec.data.ORI_QTY,
                    M_DISCPERC: T2LastRec.data.M_DISCPERC,
                    UNIT_SWAP: T2LastRec.data.UNIT_SWAP,
                    UPRICE: T2LastRec.data.UPRICE,
                    DISC_CPRICE: T2LastRec.data.DISC_CPRICE,
                    DISC_UPRICE: T2LastRec.data.DISC_UPRICE,
                    TRANSKIND: T2LastRec.data.TRANSKIND,
                    IFLAG: T2LastRec.data.IFLAG
                }]);
            T2Grid.getStore().data.items[T2Rec].data.PO_NO = "<span style='color:black'>" + T2Grid.getStore().data.items[T2Rec].data.PO_NO_REF + "</span>";
            T2Grid.getView().refresh();
            T2Rec = T2Rec + 1;
            T2LastRec = T2Grid.getStore().data.items[T2Rec];
            T2Grid.getSelectionModel().select(T2Rec, true, true);
            T2AddSubmit();
        }

        // T2 新增資料到資料庫中
        function T2AddSubmit() {
            var ACCOUNTDATE = Temp2.data.ACCOUNTDATE;
            var DELI_QTY = Temp2.data.DELI_QTY;
            var MEMO = Temp2.data.MEMO;
            var LOT_NO = Temp2.data.LOT_NO;
            var EXP_DATE = Temp2.data.EXP_DATE;
            var PO_AMT = Temp2.data.PO_AMT;
            var PO_QTY = Temp2.data.PO_QTY;
            var PO_NO = Temp2.data.PO_NO_REF;
            var MMCODE = Temp2.data.MMCODE;
            var SEQ = Temp2.data.SEQ;
            var VACCINE = Temp2.data.VACCINE;
            var CONTRACNO = Temp2.data.CONTRACNO;
            var E_PURTYPE = Temp2.data.E_PURTYPE;
            var PURDATE = Temp2.data.PURDATE;
            var AGEN_NO = Temp2.data.AGEN_NO;
            var PO_PRICE = Temp2.data.PO_PRICE;
            var STATUS = Temp2.data.STATUS;
            var M_PURUN = Temp2.data.M_PURUN;
            var WH_NO = Temp2.data.WH_NO;
            var M_DISCPERC = Temp2.data.M_DISCPERC;
            var UNIT_SWAP = Temp2.data.UNIT_SWAP;
            var UPRICE = Temp2.data.UPRICE;
            var DISC_CPRICE = Temp2.data.DISC_CPRICE;
            var DISC_UPRICE = Temp2.data.DISC_UPRICE;
            var TRANSKIND = Temp2.data.TRANSKIND;
            var IFLAG = Temp2.data.IFLAG;

            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T2Set,
                method: reqVal_p,
                params: {
                    ACCOUNTDATE: ACCOUNTDATE,
                    DELI_QTY: DELI_QTY,
                    MEMO: MEMO,
                    LOT_NO: LOT_NO,
                    EXP_DATE: EXP_DATE,
                    PO_AMT: PO_AMT,
                    PO_QTY: PO_QTY,
                    PO_NO: PO_NO,
                    MMCODE: MMCODE,
                    SEQ: SEQ,
                    VACCINE: VACCINE,
                    CONTRACNO: CONTRACNO,
                    E_PURTYPE: E_PURTYPE,
                    PURDATE: PURDATE,
                    AGEN_NO: AGEN_NO,
                    PO_PRICE: PO_PRICE,
                    STATUS: STATUS,
                    M_PURUN: M_PURUN,
                    WH_NO: WH_NO,
                    M_DISCPERC: M_DISCPERC,
                    UNIT_SWAP: UNIT_SWAP,
                    UPRICE: UPRICE,
                    DISC_CPRICE: DISC_CPRICE,
                    DISC_UPRICE: DISC_UPRICE,
                    TRANSKIND: TRANSKIND,
                    IFLAG: IFLAG
                },
                //async: true,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        msglabel('點選的資料已新增');
                        T2Grid.getSelectionModel().deselect(T2Rec - 1, true, true);
                        T2LastRec = T2Grid.getStore().data.items[T2Rec];
                        T2Grid.getStore().data.items[T2Rec].data.PO_NO = "<span style='color:blue'>" + T2Grid.getStore().data.items[T2Rec].data.PO_NO_REF + "</span>";
                        T2Grid.getStore().data.items[T2Rec].data.SEQ = data.msg.toString();
                        myMask.hide();
                        T2Grid.down('#T2commit').setDisabled(true);
                        T2Grid.down('#T2delete').setDisabled(true);
                        T2Grid.down('#T2in_pur').setDisabled(true);
                    }
                    else {
                        Ext.Msg.alert('失敗', data.msg);
                        msglabel(" " + data.msg);
                        T2Load();
                    }
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
            })
        }

        // T2 檢核進貨量
        function T2CheckQty(s) {
            
            var CHECK_FLAG = 'Y';
            var CHECK_MSG = '';
            for (var i = 0; i < T2Grid.getStore().data.items.length; i++) {
                if (parseInt(T2Grid.getStore().data.items[i].data.DELI_QTY) > parseInt(T2Grid.getStore().data.items[i].data.PO_QTY)) {
                    CHECK_FLAG = 'N';
                    CHECK_MSG += "採購單號"
                        + T2Grid.getStore().data.items[i].data.PO_NO_REF
                        + " ,院內碼"
                        + T2Grid.getStore().data.items[i].data.MMCODE
                        + ' 的進貨量'
                        + T2Grid.getStore().data.items[i].data.DELI_QTY
                        + "<span style='color:red'>不可大於</span>採購量"
                        + T2Grid.getStore().data.items[i].data.PO_QTY + "<br>";
                }
            }

            var selection = T2Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_QTY = '';
            let PO_NO = '';
            let SEQ = '';
            let STATUS = '';
            let MMCODE = '';
            $.map(selection, function (item, key) {
                ACCOUNTDATE += item.get('ACCOUNTDATE') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += item.get('EXP_DATE') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                SEQ += item.get('SEQ') + ',';
                STATUS += item.get('STATUS') + ',';
                MMCODE += item.get('MMCODE') + ',';
            })

            if (s == 'P') {
                //檢查是否有進貨日期/批號/效期未填
                var ACCOUNTDATES = ACCOUNTDATE.split(',');
                var LOT_NOS = LOT_NO.split(',');
                var EXP_DATES = EXP_DATE.split(',');
                var DELI_QTYS = DELI_QTY.split(',');
                var PO_QTYS = PO_QTY.split(',');
                var PO_NOS = PO_NO.split(',');
                var SEQS = SEQ.split(',');
                var MMCODES = MMCODE.split(',');
                var null_flag = 'Y';
                var qty_flag = 'Y';
                for (var i = 0; i < MMCODES.length - 1; i++) {
                    if (LOT_NOS[i] == '' || EXP_DATES[i] == '') {
                        null_flag = 'N';
                        Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        msglabel("勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");

                        break;
                    }
                    if (parseInt(DELI_QTYS[i]) < parseInt(PO_QTYS[i])) {
                        qty_flag = 'N';
                    }
                }
            }
            
            if ((null_flag == 'Y') && (qty_flag == 'N')) {
                Ext.MessageBox.confirm('提醒', "勾選的資料中有<span style='color:red'>進貨量 < 採購量</span>，是否確定執行進貨？", function (btn, text) {
                    if (btn === 'yes') {
                        if ((CHECK_FLAG == 'Y') && (s == 'C')) {
                            T2Submit('C');
                        }
                        else if ((CHECK_FLAG == 'Y') && (s == 'P')) {
                            T2Submit('P');
                        }
                        else {
                            Ext.Msg.alert("提醒", CHECK_MSG);
                            msglabel(CHECK_MSG);
                        }
                    }
                });
            }
            else if ((null_flag == 'Y') && (qty_flag == 'Y')) {
                if ((CHECK_FLAG == 'Y') && (s == 'C')) {
                    T2Submit('C');
                }
                else if ((CHECK_FLAG == 'Y') && (s == 'P')) {
                    T2Submit('P');
                }
                else {
                    Ext.Msg.alert("提醒", CHECK_MSG);
                    msglabel(CHECK_MSG);
                }
            }

            
        }

        // T2 進貨檢查
        function T2in_pur() {
            var selection = T2Grid.getSelection();
            let ACCOUNTDATE = '';
            let DELI_QTY = '';
            let LOT_NO = '';
            let EXP_DATE = '';
            let PO_QTY = '';
            let PO_NO = '';
            let SEQ = '';
            let STATUS = '';
            let MMCODE = '';
            $.map(selection, function (item, key) {
                ACCOUNTDATE += item.get('ACCOUNTDATE') + ',';
                DELI_QTY += item.get('DELI_QTY') + ',';
                LOT_NO += item.get('LOT_NO') + ',';
                EXP_DATE += item.get('EXP_DATE') + ',';
                PO_QTY += item.get('PO_QTY') + ',';
                PO_NO += item.get('PO_NO_REF') + ',';
                SEQ += item.get('SEQ') + ',';
                STATUS += item.get('STATUS') + ',';
                MMCODE += item.get('MMCODE') + ',';
            })
            if (STATUS.indexOf('Y') >= 0) {
                Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>已進貨的資料</span>，無法執行進貨，請檢核");
                msglabel("勾選的資料中有<span style='color:red'>已進貨的資料</span>，無法執行進貨，請檢核");
                T2Grid.down('#T2commit').setDisabled(false);
                T2Grid.down('#T2delete').setDisabled(false);
                T2Grid.down('#T2in_pur').setDisabled(false);
            }
            else {
                //檢查是否有進貨日期/批號/效期未填
                var ACCOUNTDATES = ACCOUNTDATE.split(',');
                var LOT_NOS = LOT_NO.split(',');
                var EXP_DATES = EXP_DATE.split(',');
                var DELI_QTYS = DELI_QTY.split(',');
                var PO_QTYS = PO_QTY.split(',');
                var PO_NOS = PO_NO.split(',');
                var SEQS = SEQ.split(',');
                var MMCODES = MMCODE.split(',');
                var null_flag = 'Y';
                var qty_flag = 'Y';
                for (var i = 0; i < MMCODES.length - 1; i++) {
                    if (LOT_NOS[i] == '' || EXP_DATES[i] == '') {
                        null_flag = 'N';
                        Ext.Msg.alert('提醒', "勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        msglabel("勾選的資料中有<span style='color:red'>批號或效期</span>未填，無法執行進貨，請檢核");
                        T2Grid.down('#T2commit').setDisabled(false);
                        T2Grid.down('#T2delete').setDisabled(false);
                        T2Grid.down('#T2in_pur').setDisabled(false);
                        break;
                    }
                    if (parseInt(DELI_QTYS[i]) < parseInt(PO_QTYS[i])) {
                        qty_flag = 'N';
                    }
                }
                if ((null_flag == 'Y') && (qty_flag == 'N')) {
                    Ext.MessageBox.confirm('提醒', "勾選的資料中有<span style='color:red'>進貨量 < 採購量</span>，是否確定執行進貨？", function (btn, text) {
                        if (btn === 'yes') {
                            T2in_pur_submit(PO_NO, SEQ, MMCODE);
                        }
                    });
                }
                else if ((null_flag == 'Y') && (qty_flag == 'Y')) {
                    T2in_pur_submit(PO_NO, SEQ, MMCODE);
                }
            }
        }

        // T2 確認進貨
        function T2in_pur_submit(PO_NO, SEQ, MMCODE) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            Ext.Ajax.request({
                url: T2Set,
                method: reqVal_p,
                params: {
                    PO_NO: PO_NO,
                    SEQ: SEQ,
                    MMCODE: MMCODE
                },
                //async: true,
                success: function (response) {
                    myMask.hide();
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        msglabel('點選的資料已執行進貨');
                        T2LastRec = null;
                        T2Load();
                        myMask.hide();
                        T2Grid.down('#T2commit').setDisabled(true);
                        T2Grid.down('#T2delete').setDisabled(true);
                        T2Grid.down('#T2in_pur').setDisabled(true);
                    }
                    else {
                        Ext.Msg.alert('失敗', data.msg);
                        msglabel(" " + data.msg);
                        T2Load();
                    }
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
            })
        }
    }
    // #endregion
    //#region T3
    // T3 查詢欄位
    var T3QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70,
            width: 180
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP31',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫別',
                        name: 'P0',
                        labelWidth: 50,
                        width: 200,
                        store: WH_NO_Store,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        fieldCls: 'required',
                        allowBlank: false, // 欄位是否為必填
                        blankText: "請輸入庫別",
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '廠商代碼',
                        name: 'P1',
                        labelWidth: 70,
                        width: 220,
                        store: AGEN_NO_Store,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        allowBlank: true, // 欄位是否為必填
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        //}, {
                        //    xtype: 'datefield',
                        //    fieldLabel: '採購日期',
                        //    name: 'P2',
                        //    id: 'P2',
                        //    labelWidth: 70,
                        //    width: 170,
                        //    value: new Date(),
                        //    enforceMaxLength: true,
                        //    fieldCls: 'required',
                        //    allowBlank: false, // 欄位是否為必填
                        //    blankText: "請輸入採購日期",
                        //    renderer: function (value, meta, record) {
                        //        return Ext.util.Format.date(value, 'X/m/d');
                        //    }
                        //}, {
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '採購日期',
                        name: 'P2',
                        labelWidth: 70,
                        width: 150,
                        padding: '0 4 0 4',
                        fieldCls: 'required',
                        vtype: 'dateRange',
                        dateRange: { end: 'P2_1' },
                        value: Ext.Date.add(new Date(), Ext.Date.DAY, -3)
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P2_1',
                        labelWidth: 8,
                        width: 88,
                        padding: '0 4 0 4',
                        labelSeparator: '',
                        fieldCls: 'required',
                        vtype: 'dateRange',
                        dateRange: { begin: 'P2' },
                        value: new Date()
                    }, {
                        xtype: 'combo',
                        fieldLabel: '類別',
                        name: 'P3',
                        labelWidth: 50,
                        width: 150,
                        store: KIND_Store,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        fieldCls: 'required',
                        allowBlank: false, // 欄位是否為必填
                        blankText: "請輸入類別",
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    },
                    {
                        xtype: 'button',
                        itemId: 'query',
                        text: '查詢',
                        handler: function () {
                            T3Load();
                            msglabel("");
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'clean',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            T3QueryForm.getForm().findField("P0").setValue(first_wh_no);
                            T3QueryForm.getForm().findField("P1").setValue('');
                            T3QueryForm.getForm().findField("P2").setValue(Ext.Date.add(new Date(), Ext.Date.DAY, -3));
                            T3QueryForm.getForm().findField("P2_1").setValue(new Date());
                            T3QueryForm.getForm().findField("P3").setValue('');
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }]
            }]
    });

    // T3 Store
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'WEBAPP.model.CC0003',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'AGEN_NO_NAME', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CC0003/All_3',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T3QueryForm.getForm().findField('P0').getValue(),
                    p1: T3QueryForm.getForm().findField('P1').getValue(),
                    p2: T3QueryForm.getForm().findField('P2').getRawValue(),
                    p2_1: T3QueryForm.getForm().findField('P2_1').getRawValue(),
                    p3: T3QueryForm.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // T3 toolbar,包含換頁、新增/修改/刪除鈕
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // T3 查詢結果資料列表
    var T3Grid = Ext.create('Ext.grid.Panel', {
        //title: '',
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',
        dockedItems: [
            {
                items: [T3QueryForm]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],
        columns: [{
            dataIndex: 'ACCOUNTDATE_REF',
            text: "原進貨日期",
            hidden: true
        }, {
            dataIndex: 'DELI_QTY_REF,',
            text: "原進貨量",
            hidden: true
        }, {
            dataIndex: 'LOT_NO_REF',
            text: "原批號",
            hidden: true
        }, {
            dataIndex: 'EXP_DATE_REF',
            text: "原效期",
            hidden: true
        }, {
            dataIndex: 'MEMO_REF',
            text: "原備註",
            hidden: true
        }, {
            dataIndex: 'SEQ',
            text: "流水號",
            hidden: true
        }, {
            dataIndex: 'PO_NO_REF',
            text: "採購單號",
            hidden: true
        }, {
            dataIndex: 'M_DISCPERC',
            text: "折讓比",
            hidden: true
        }, {
            dataIndex: 'UNIT_SWAP',
            text: "轉換率",
            hidden: true
        }, {
            dataIndex: 'UPRICE',
            text: "最小單價",
            hidden: true
        }, {
            dataIndex: 'DISC_CPRICE',
            text: "優惠合約單價",
            hidden: true
        }, {
            dataIndex: 'DISC_UPRICE',
            text: "優惠最小單價",
            hidden: true
        }, {
            dataIndex: 'TRANSKIND',
            text: "異動類別",
            hidden: true
        }, {
            dataIndex: 'IFLAG',
            text: "新增識別",
            hidden: true
        }, {
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "疫苗",
            dataIndex: 'VACCINE',
            width: 40,
            sortable: true
        }, {
            text: "合約碼",
            dataIndex: 'CONTRACNO',
            width: 50,
            sortable: true
        }, {
            text: "案別",
            dataIndex: 'E_PURTYPE',
            width: 40,
            sortable: true
        }, {
            text: "採購日期",
            dataIndex: 'PURDATE',
            width: 60,
            sortable: true
        }, {
            text: "採購單號",
            dataIndex: 'PO_NO',
            width: 90,
            sortable: true
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO_NAME',
            width: 150,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 70,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200,
            sortable: true
        }, {
            text: "進貨日期",
            dataIndex: 'ACCOUNTDATE',
            width: 70,
            sortable: true,       
        }, {
            text: "採購量",
            dataIndex: 'PO_QTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            format: '0'
        }, {
            text: "進貨量",
            dataIndex: 'DELI_QTY',
            width: 80,
            sortable: true,
            align: 'right',
        }, {
            text: "進貨",
            dataIndex: 'INFLAG',
            width: 50,
            sortable: true
        }, {
            text: "退貨",
            dataIndex: 'OUTFLAG',
            width: 50,
            sortable: true
        }, {
            text: "單價",
            dataIndex: 'PO_PRICE',
            width: 70,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            format: '0.00'
        }, {
            text: "總金額",
            dataIndex: 'PO_AMT',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            format: '0.00'
        }, {
            text: "進貨單位",
            dataIndex: 'M_PURUN',
            width: 70,
            sortable: true
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 120,
            sortable: true,
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 100,
            sortable: true,
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 120,
            sortable: true,
        }, {
            text: "庫房別",
            dataIndex: 'WH_NO',
            width: 70,
            sortable: true
        }, {
            text: "狀態",
            dataIndex: 'STATUS',
            width: 50,
            sortable: true
        }, {
            text: "異動類別",
            dataIndex: 'TRANSKIND',
            width: 50,
            sortable: true
        }, {
            text: "流水號",
            dataIndex: 'SEQ',
            width: 50,
            sortable: true
        }/*, {
                text: "進貨量",
                dataIndex: 'ORI_QTY',
                width: 60,
                sortable: true,
                xtype: 'numbercolumn',
                align: 'right',
                style: 'text-align:left',
                format: '0'
            }*/, {
            header: "",
            flex: 1
        }],
    });
//#endregion
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義畫面 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var TATabs = Ext.widget('tabpanel', {
            listeners: {
                tabchange: function (tabpanel, newCard, oldCard) {
                    switch (newCard.title) {
                        case "進貨確認":
                            //T1Form.setVisible(true);
                            //T2Form.setVisible(false);
                            //T3Form.setVisible(false);
                            T1QueryForm.getForm().findField('P0').focus();
                            break;
                        case "下個月入帳資料":
                            //T1Form.setVisible(true);
                            //T2Form.setVisible(false);
                            //T3Form.setVisible(false);
                            T2QueryForm.getForm().findField('P4').focus();
                            T1QueryForm.getForm().findField('P0').clearInvalid();
                            T1QueryForm.getForm().findField('P1').clearInvalid();
                            T1QueryForm.getForm().findField('P2').clearInvalid();
                            T1QueryForm.getForm().findField('P3').clearInvalid();
                            break;
                        case "訂單進貨資料":
                            //T1Form.setVisible(true);
                            //T2Form.setVisible(false);
                            //T3Form.setVisible(false);
                            T3QueryForm.getForm().findField('P0').focus();
                            break;
                    }
                }
            },
            layout: 'fit',
            plain: true,
            border: false,
            resizeTabs: true,       //改變tab尺寸       
            enableTabScroll: true,  //是否允許Tab溢出時可以滾動
            defaults: {
                // autoScroll: true,
                closabel: false,    //tab是否可關閉
                padding: 0,
                split: true
            },
            items: [{
                itemId: 't1Grid',
                title: '進貨確認',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                title: '下個月入帳資料',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T2Grid]
                }, {
                    itemId: 't3Grid',
                    title: '訂單進貨資料',
                    layout: 'border',
                    padding: 0,
                    split: true,
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    border: false,
                    items: [T3Grid]
                }]
        });

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
                itemId: 't1Form',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [TATabs]
            }
            ]
        });
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ LOAD ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        function T1Load() {
            T1Rec = '';
            T1LastRec = null;
            T1Grid.down('#T1add').setDisabled(true);
            T1Tool.moveFirst();
        }

        function T2Load() {
            T2Rec = '';
            T2LastRec = null;
            T2Grid.down('#T2add').setDisabled(true);
            T2Tool.moveFirst();
        }

        function T3Load() {
            T3Rec = '';
            T3LastRec = null;
            T3Tool.moveFirst();
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 起始執行 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    T1QueryForm.getForm().findField('P0').focus();
    SetWH_NO();
    SetAGEN_NO();
    SetKIND();
});
